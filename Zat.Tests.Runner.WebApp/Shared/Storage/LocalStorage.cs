namespace Zat.Tests.Runner.WebApp.Shared.Storage;

using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

public class LocalStorage<TData>(
    ProtectedLocalStorage protectedLocalStorage,
    ILogger logger,
    Func<TData> fallbackFactory)
    where TData : class
{
    private const string KeyPrefix = "Zat.Tests.Runner";

    public async Task<TData> GetItemAsync(string key)
    {
        try
        {
            var result =
                await protectedLocalStorage.GetAsync<TData>(BuildKey(key));

            return result.Success
                ? result.Value ?? fallbackFactory()
                : fallbackFactory();
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.Log(
                LogLevel.Warning,
                "The state remembered by the browser could not be read." +
                "\nException: {Exception}",
                exception);

            return fallbackFactory();
        }
    }

    public async Task SetItemAsync(string key, TData data)
    {
        try
        {
            await protectedLocalStorage.SetAsync(BuildKey(key), data);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.Log(
                LogLevel.Warning,
                "The current state could not be remembered by the browser, so it is lost on reload." +
                "\nException: {Exception}",
                exception);
        }
    }

    public async Task DeleteItemAsync(string key)
    {
        try
        {
            await protectedLocalStorage.DeleteAsync(BuildKey(key));
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.Log(
                LogLevel.Warning,
                "The current state could not be deleted from the browser storage." +
                "\nException: {Exception}",
                exception);
        }
    }

    private static string BuildKey(string key)
        => $"{KeyPrefix}.{key}";
}