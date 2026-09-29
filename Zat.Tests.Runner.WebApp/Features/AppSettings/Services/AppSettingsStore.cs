namespace Zat.Tests.Runner.WebApp.Features.AppSettings.Services;

using Microsoft.Extensions.Hosting;

using Zat.Tests.Runner.WebApp.Application.Paths;
using Zat.Tests.Runner.Common.Net.Logging;
using Zat.Tests.Runner.WebApp.Shared.Storage;
using Zat.Tests.Runner.WebApp.Shared.Stores;
using Zat.Tests.Runner.WebApp.Shared.Stores.AppSettings;

/// <summary>
/// Store of application settings shared by every browser and backed by a file.
/// </summary>
/// <remarks>
/// Shared by every browser because the settings describe the test machine rather than one browser.
/// <para>
/// Reading runs during startup, so <see cref="StoreBase{TState}.Current"/> is ready before the
/// first browser connects.
/// </para>
/// </remarks>
/// <param name="paths">Provider of the application paths, naming the settings file.</param>
/// <param name="logger">Log the failures of the settings file are reported to.</param>
public sealed class AppSettingsStore(IAppPathsProvider paths, IAppLogger logger)
    : StoreBase<AppSettingsState>, IAppSettingsStore, IHostedService
{
    /// <summary>
    /// Default IDE install folder when no application settings were saved.
    /// </summary>
    public const string DefaultIdeInstallFolderPath = @"C:\Program Files (x86)\Pertinax6";

    private readonly JsonFileStorage<AppSettingsState> storage = new(
        paths.Files.UserSettings,
        logger,
        static () => Defaults);

    private static AppSettingsState Defaults => new(DefaultIdeInstallFolderPath);

    /// <inheritdoc/>
    protected override AppSettingsState DefaultState => Defaults;

    /// <summary>
    /// Restores the saved application settings.
    /// </summary>
    /// <remarks>
    /// Read failures leave the store on the defaults and are reported to the log.
    /// </remarks>
    /// <param name="cancellationToken">Token abandoning the start.</param>
    /// <returns>A task that completes after the application settings have been read.</returns>
    public async Task StartAsync(CancellationToken cancellationToken)
        => this.SetState(await this.storage.ReadAsync());

    /// <inheritdoc/>
    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;

    /// <summary>
    /// Saves updated application settings and publishes them once the file write succeeds.
    /// </summary>
    /// <remarks>
    /// The file is written before listeners are told. A failed write leaves the store unchanged, so
    /// the current settings still match what a restart would read.
    /// </remarks>
    /// <param name="update">Produces new application settings from the current ones.</param>
    /// <returns><see langword="true"/> when the updated application settings were saved.</returns>
    public async Task<bool> SaveAsync(Func<AppSettingsState, AppSettingsState> update)
    {
        ArgumentNullException.ThrowIfNull(update);

        var updated = update(this.Current);

        if (!await this.storage.WriteAsync(updated))
        {
            return false;
        }

        await base.UpdateAsync(_ => updated);

        return true;
    }

    /// <inheritdoc/>
    public override Task UpdateAsync(Func<AppSettingsState, AppSettingsState> update)
        => this.SaveAsync(update);
}