namespace Zat.Tests.Runner.WebApp.Application.Logging;

using System.Globalization;

/// <summary>
/// Naming and retention rules for daily log files.
/// </summary>
public static class LogFile
{
    /// <summary>
    /// File extension of every log file.
    /// </summary>
    public const string Extension = ".log";

    private const string DateFormat = "yyyy-MM-dd";

    /// <summary>
    /// Gets the log file path for <paramref name="timestamp"/>.
    /// </summary>
    /// <param name="directoryPath">Directory holding the log files.</param>
    /// <param name="timestamp">Time of the log entry.</param>
    /// <returns>Path of the log file for <paramref name="timestamp"/>.</returns>
    public static string GetPath(string directoryPath, DateTimeOffset timestamp)
    {
        var fileName = timestamp.ToString(DateFormat, CultureInfo.InvariantCulture) + Extension;

        return Path.Combine(directoryPath, fileName);
    }

    /// <summary>
    /// Enumerates the log files, newest first.
    /// </summary>
    /// <remarks>
    /// Files are recognized by parsing the whole file name as a date so retention cannot delete
    /// unrelated files.
    /// </remarks>
    /// <param name="directoryPath">Directory holding the log files.</param>
    /// <returns>Log file paths, newest first.</returns>
    public static IEnumerable<string> Enumerate(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            return [];
        }

        // The date is the whole file name, so ordinal ordering by name is chronological.
        return Directory
            .EnumerateFiles(directoryPath)
            .Where(IsLogFile)
            .OrderByDescending(Path.GetFileName, StringComparer.Ordinal);
    }

    /// <summary>
    /// Deletes log files older than the retained set.
    /// </summary>
    /// <remarks>
    /// Retention counts files, not calendar days.
    /// </remarks>
    /// <param name="directoryPath">Directory holding the log files.</param>
    /// <param name="retainedFileCount">Number of log files to keep.</param>
    public static void ApplyRetention(string directoryPath, int retainedFileCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(retainedFileCount);

        // Materialized before deleting, so that the enumeration is not invalidated underneath.
        List<string> obsoleteFilePaths = [..Enumerate(directoryPath).Skip(retainedFileCount)];

        foreach (var filePath in obsoleteFilePaths)
        {
            File.Delete(filePath);
        }
    }

    private static bool IsLogFile(string filePath)
    {
        if (!Path.GetExtension(filePath).Equals(Extension, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return DateOnly.TryParseExact(
            Path.GetFileNameWithoutExtension(filePath),
            DateFormat,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out _);
    }
}