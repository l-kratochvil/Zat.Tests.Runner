namespace Zat.Tests.Runner.WebApp.Shared.Stores;

using Fluxor.Persist.Storage;

using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

using Zat.Tests.Runner.Common.Net.Logging;
using Zat.Tests.Runner.WebApp.Shared.Storage;

public class LocalStringStateStorage(
    ProtectedLocalStorage protectedLocalStorage,
    IAppLogger logger)
    : IStringStateStorage
{
    private readonly LocalStorage<string> localStorage = new(
        protectedLocalStorage, logger, () => string.Empty);

    /// <inheritdoc/>
    public async ValueTask<string> GetStateJsonAsync(string statename)
        => await this.localStorage.GetItemAsync(statename);

    /// <inheritdoc/>
    public async ValueTask StoreStateJsonAsync(string statename, string json)
        => await this.localStorage.SetItemAsync(statename, json);
}