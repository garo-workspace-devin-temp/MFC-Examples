using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using FtpClient.UI.Models;

namespace FtpClient.UI.Services;

public static class FtpListParser
{
    private static readonly Regex UnixListRegex = new Regex(
        @"^([d\-lrwxst]+)\s+(\d+)\s+(\S+)\s+(\S+)\s+(\d+)\s+(\w{3}\s+\d{1,2}\s+(?:\d{4}|\d{1,2}:\d{2}))\s+(.+)$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex DosListRegex = new Regex(
        @"^(\d{2}-\d{2}-\d{2})\s+(\d{2}:\d{2}[AP]M)\s+(<DIR>|\d+)\s+(.+)$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static List<FtpFileInfo> ParseListResponse(string listResponse, string currentPath)
    {
        var files = new List<FtpFileInfo>();
        if (string.IsNullOrWhiteSpace(listResponse))
            return files;

        var lines = listResponse.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        
        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            if (string.IsNullOrEmpty(trimmedLine))
                continue;

            var fileInfo = ParseUnixFormat(trimmedLine, currentPath) ?? 
                          ParseDosFormat(trimmedLine, currentPath) ?? 
                          ParseGenericFormat(trimmedLine, currentPath);

            if (fileInfo != null)
            {
                files.Add(fileInfo);
            }
        }

        return files;
    }

    private static FtpFileInfo? ParseUnixFormat(string line, string currentPath)
    {
        var match = UnixListRegex.Match(line);
        if (!match.Success)
            return null;

        var permissions = match.Groups[1].Value;
        var linkCount = match.Groups[2].Value;
        var owner = match.Groups[3].Value;
        var group = match.Groups[4].Value;
        var size = long.Parse(match.Groups[5].Value);
        var dateStr = match.Groups[6].Value;
        var name = match.Groups[7].Value;

        var type = FtpFileType.File;
        if (permissions.StartsWith("d"))
            type = FtpFileType.Directory;
        else if (permissions.StartsWith("l"))
            type = FtpFileType.Link;

        var fullPath = CombinePath(currentPath, name);
        var modifiedTime = ParseUnixDate(dateStr);

        return new FtpFileInfo(name, fullPath, type)
        {
            Size = type == FtpFileType.Directory ? 0 : size,
            ModifiedTime = modifiedTime,
            Permissions = permissions,
            Owner = owner,
            Group = group,
            RawListing = line
        };
    }

    private static FtpFileInfo? ParseDosFormat(string line, string currentPath)
    {
        var match = DosListRegex.Match(line);
        if (!match.Success)
            return null;

        var dateStr = match.Groups[1].Value;
        var timeStr = match.Groups[2].Value;
        var sizeOrDir = match.Groups[3].Value;
        var name = match.Groups[4].Value;

        var type = sizeOrDir == "<DIR>" ? FtpFileType.Directory : FtpFileType.File;
        var size = type == FtpFileType.Directory ? 0 : long.Parse(sizeOrDir);
        var fullPath = CombinePath(currentPath, name);
        var modifiedTime = ParseDosDate(dateStr, timeStr);

        return new FtpFileInfo(name, fullPath, type)
        {
            Size = size,
            ModifiedTime = modifiedTime,
            RawListing = line
        };
    }

    private static FtpFileInfo? ParseGenericFormat(string line, string currentPath)
    {
        if (string.IsNullOrWhiteSpace(line))
            return null;

        var name = line.Trim();
        var fullPath = CombinePath(currentPath, name);

        return new FtpFileInfo(name, fullPath, FtpFileType.Unknown)
        {
            RawListing = line
        };
    }

    private static DateTime? ParseUnixDate(string dateStr)
    {
        try
        {
            var parts = dateStr.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 3)
                return null;

            var monthStr = parts[0];
            var dayStr = parts[1];
            var yearOrTimeStr = parts[2];

            var month = ParseMonth(monthStr);
            if (month == 0)
                return null;

            if (!int.TryParse(dayStr, out var day))
                return null;

            var currentYear = DateTime.Now.Year;
            int year;
            int hour = 0, minute = 0;

            if (yearOrTimeStr.Contains(':'))
            {
                var timeParts = yearOrTimeStr.Split(':');
                if (timeParts.Length == 2 && 
                    int.TryParse(timeParts[0], out hour) && 
                    int.TryParse(timeParts[1], out minute))
                {
                    year = currentYear;
                    var testDate = new DateTime(year, month, day, hour, minute, 0);
                    if (testDate > DateTime.Now)
                        year--;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                if (!int.TryParse(yearOrTimeStr, out year))
                    return null;
            }

            return new DateTime(year, month, day, hour, minute, 0);
        }
        catch
        {
            return null;
        }
    }

    private static DateTime? ParseDosDate(string dateStr, string timeStr)
    {
        try
        {
            var dateTime = DateTime.ParseExact($"{dateStr} {timeStr}", 
                "MM-dd-yy hh:mmtt", CultureInfo.InvariantCulture);
            
            if (dateTime.Year < 1980)
                dateTime = dateTime.AddYears(100);
                
            return dateTime;
        }
        catch
        {
            return null;
        }
    }

    private static int ParseMonth(string monthStr)
    {
        return monthStr.ToLowerInvariant() switch
        {
            "jan" => 1,
            "feb" => 2,
            "mar" => 3,
            "apr" => 4,
            "may" => 5,
            "jun" => 6,
            "jul" => 7,
            "aug" => 8,
            "sep" => 9,
            "oct" => 10,
            "nov" => 11,
            "dec" => 12,
            _ => 0
        };
    }

    private static string CombinePath(string basePath, string name)
    {
        if (string.IsNullOrEmpty(basePath) || basePath == "/")
            return "/" + name;
        
        return basePath.TrimEnd('/') + "/" + name;
    }
}
