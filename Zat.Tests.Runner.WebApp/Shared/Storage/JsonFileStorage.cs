namespace Zat.Tests.Runner.WebApp.Shared.Storage;

using System.IO;
using System.Text.Json;

using Zat.Tests.Runner.Common.Net.Logging;

/// <summary>
/// Keeps one shared value in a JSON file on the test machine.
/// </summary>
/// <remarks>
/// Unlike <see cref="LocalStorage{TData}"/>, this storage belongs to the installation rather than
/// to one browser.
/// <para>
/// Reading never fails. A missing, unreadable, or invalid file falls back to the value from
/// <paramref name="fallbackFactory"/> and reports the failure to the log.
/// </para>
/// </remarks>
/// <typeparam name="TData">Shape of the value kept in the file.</typeparam>
/// <param name="filePath">Full path of the file.</param>
/// <param name="logger">Log read and write failures are reported to.</param>
/// <param name="fallbackFactory">Produces the fallback value when the file cannot be read.</param>
public class JsonFileStorage<TData>(
    string filePath,
    IAppLogger logger,
    Func<TData> fallbackFactory)
    where TData : class
{
    private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = true };

    /// <summary>
    /// Reads the stored value.
    /// </summary>
    /// <returns>The stored value, or the fallback when the file cannot be read.</returns>
    public async Task<TData> ReadAsync()
    {
        try
        {
            if (!File.Exists(filePath))
            {
                return fallbackFactory();
            }

            await using var stream = File.OpenRead(filePath);

            return await JsonSerializer.DeserializeAsync<TData>(stream, SerializerOptions)
                   ?? fallbackFactory();
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.Warning(
                $"The settings file could not be read, so the default settings are used ({filePath}).",
                exception.ToString());

            return fallbackFactory();
        }
    }

    /// <summary>
    /// Writes <paramref name="data"/> to the file.
    /// </summary>
    /// <param name="data">Value to write.</param>
    /// <returns><see langword="true"/> when <paramref name="data"/> reached the file.</returns>
    public async Task<bool> WriteAsync(TData data)
    {
        try
        {
            var directoryPath = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            await using var stream = File.Create(filePath);
            await JsonSerializer.SerializeAsync(stream, data, SerializerOptions);

            return true;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.Error(
                $"The settings could not be saved, so they are lost when the application stops ({filePath}).",
                exception.ToString());

            return false;
        }
    }
}