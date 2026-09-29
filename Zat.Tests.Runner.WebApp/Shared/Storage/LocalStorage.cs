namespace Zat.Tests.Runner.WebApp.Shared.Storage;

using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

using Zat.Tests.Runner.Common.Net.Logging;

public class LocalStorage<TData>(
    ProtectedLocalStorage protectedLocalStorage,
    IAppLogger logger,
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
            logger.Warning(
                "The state remembered by the browser could not be read.",
                exception.ToString());

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
            logger.Warning(
                "The current state could not be remembered by the browser, so it is lost on reload.",
                exception.ToString());
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
            logger.Warning(
                "The current state could not be deleted from the browser storage.",
                exception.ToString());
        }
    }

    private static string BuildKey(string key)
        => $"{KeyPrefix}.{key}";
}