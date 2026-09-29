namespace Zat.Tests.Runner.WebApp.Shared.Storage;

using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Zat.Tests.Runner.Common.Net.Logging;

/// <typeparam name="TData">Shape of the remembered value.</typeparam>
/// <param name="storageName">Key <typeparamref name="TData"/> is remembered under.</param>
/// <param name="protectedLocalStorage">Browser storage <typeparamref name="TData"/> is kept in.</param>
/// <param name="logger">Log storage failures are reported to.</param>
/// <param name="fallbackFactory">Produces the value returned when nothing is remembered yet.</param>
public class NamedLocalStorage<TData>(
    string storageName,
    ProtectedLocalStorage protectedLocalStorage,
    IAppLogger logger,
    Func<TData> fallbackFactory)
    where TData : class
{
    private readonly LocalStorage<TData> localStorage = new(
        protectedLocalStorage, logger, fallbackFactory);

    /// <summary>
    /// Reads the value remembered in the browser.
    /// </summary>
    /// <returns>The remembered value.</returns>
    public async Task<TData> ReadAsync()
        => await this.localStorage.GetItemAsync(storageName);

    /// <summary>
    /// Asks the browser to remember <paramref name="data"/>.
    /// </summary>
    /// <param name="data">Value to remember.</param>
    /// <returns>A task that completes once the browser has stored <paramref name="data"/>.</returns>
    public async Task WriteAsync(TData data)
        => await this.localStorage.SetItemAsync(storageName, data);
}