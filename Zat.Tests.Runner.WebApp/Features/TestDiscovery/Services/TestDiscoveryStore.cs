namespace Zat.Tests.Runner.WebApp.Features.TestDiscovery.Services;

using System.Linq;

using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

using Zat.Tests.Runner.Common.Model;
using Zat.Tests.Runner.Common.Net.Logging;
using Zat.Tests.Runner.WebApp.Shared.Storage;
using Zat.Tests.Runner.WebApp.Shared.Stores;
using Zat.Tests.Runner.WebApp.Shared.Stores.TestDiscovery;

/// <summary>
/// The test selection being put together. Selecting happens in one browser tab and belongs to it,
/// so the store is scoped to a circuit; what the user selected last is remembered across reloads.
/// </summary>
/// <remarks>
/// Only this feature is handed the store itself; everyone else is handed
/// <see cref="ITestDiscoveryStore"/>, which reads and cannot select.
/// </remarks>
/// <param name="logger">Log a storage failure is reported to.</param>
/// <param name="protectedLocalStorage">Browser storage the selection is remembered in.</param>
public sealed class TestDiscoveryStore(
    IAppLogger logger,
    ProtectedLocalStorage protectedLocalStorage)
    : StoreBase<TestDiscoveryState>, ITestDiscoveryStore
{
    private readonly NamedLocalStorage<LocalStorageData> localStorage =
        new("test-discovery", protectedLocalStorage, logger, () => new LocalStorageData([]));

    /// <inheritdoc/>
    protected override TestDiscoveryState DefaultState
        => new(SelectedTestCases: []);

    /// <summary>
    /// Reads the execution paths of the test cases the browser remembers being selected.
    /// </summary>
    /// <remarks>
    /// Paths rather than test cases, because turning a path back into a test case needs the test
    /// tree, which this store deliberately knows nothing about. Whoever holds the tree resolves
    /// them and hands the result back to <see cref="RestoreSelection"/>.
    /// </remarks>
    /// <returns>The remembered execution paths, empty when there are none.</returns>
    public async Task<IReadOnlyList<string>> ReadRememberedPathsAsync()
    {
        var data = await this.localStorage.ReadAsync();
        return data.SelectedTestCasesPaths;
    }

    /// <summary>
    /// Puts back a selection that was remembered, without remembering it again.
    /// </summary>
    /// <remarks>
    /// Writing here would store what was just read, which is a change nobody made and which would
    /// hide a later mistake in the order things happen.
    /// </remarks>
    /// <param name="selectedTestCases">Test cases the remembered paths resolved to.</param>
    public void RestoreSelection(IEnumerable<TestCaseEntity> selectedTestCases)
        => this.SetState(new TestDiscoveryState(SelectedTestCases: [..selectedTestCases]));

    /// <summary>
    /// Changes the selection and has the browser remember it.
    /// </summary>
    /// <param name="update">Produces the new selection from the current one.</param>
    /// <returns>A task that completes once the browser has stored the selection.</returns>
    public override async Task UpdateAsync(Func<TestDiscoveryState, TestDiscoveryState> update)
    {
        await base.UpdateAsync(update);

        await this.localStorage.WriteAsync(
            new LocalStorageData(
                [..this.Current.SelectedTestCases.Select(testCase => testCase.ExecutionPath)]));
    }

    private sealed record LocalStorageData(string[] SelectedTestCasesPaths);
}