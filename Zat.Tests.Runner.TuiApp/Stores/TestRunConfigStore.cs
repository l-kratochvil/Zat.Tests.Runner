namespace Zat.Tests.Runner.TuiApp.Stores;

using Zat.Tests.Runner.Common.Model;
using Zat.Tests.Runner.Common.Net.Services;
using Zat.Z2xxTests.Common.Model;

/// <summary>
/// This is the store for test run config that is used by the running test.
/// </summary>
/// <remarks>
/// The tests to choose from follow <see cref="IExploredTestAssemblyStore"/>; once the test assembly holds other test entities,
/// the test selection is cleared, as it may no longer be what the user selected.
/// The test selection is kept as execution paths in the application state, so it survives a restart; those the test
/// tree no longer holds are skipped.
/// </remarks>
internal class TestRunConfigStore
{
    private readonly AppStateStore appStateStore;
    private readonly IExploredTestAssemblyStore exploredTestAssemblyStore;

    public TestRunConfigStore(AppStateStore appStateStore, IExploredTestAssemblyStore exploredTestAssemblyStore)
    {
        this.appStateStore = appStateStore;
        this.exploredTestAssemblyStore = exploredTestAssemblyStore;

        // Both are singletons, so the subscription lasts as long as the application does.
        this.exploredTestAssemblyStore.Changed += this.OnTestTreeChanged;
    }

    public IEnumerable<TestSuiteEntity> LoadedTestSuites
        => this.exploredTestAssemblyStore.TestSuites;

    public IEnumerable<TestEntity> SelectedTestEntities
    {
        get => FindTestEntities(this.LoadedTestSuites, this.appStateStore.Current.SelectedExecutionPaths);
        set => this.appStateStore.Update(
            current => current with { SelectedExecutionPaths = [.. value.Select(entity => entity.ExecutionPath)] });
    }

    public bool RuntimeTestEntitiesSelected
        => this.SelectedTestEntities.Any(x => x.TestType is TestType.Runtime);

    public bool? IsTestLinkReportingEnabled
    {
        get => this.appStateStore.Current.IsTestLinkReportingEnabled;
        set => this.appStateStore.Update(
            current => current with { IsTestLinkReportingEnabled = value });
    }

    public bool? IsEmailNotificationEnabled
    {
        get => this.appStateStore.Current.IsEmailNotificationEnabled;
        set => this.appStateStore.Update(
            current => current with { IsEmailNotificationEnabled = value });
    }

    public string[] EmailAddresses
    {
        get => this.appStateStore.Current.EmailAddresses;
        set => this.appStateStore.Update(
            current => current with { EmailAddresses = value });
    }

    public bool? IsDebugModeEnabled
    {
        get => this.appStateStore.Current.IsDebugModeEnabled;
        set => this.appStateStore.Update(
            current => current with { IsDebugModeEnabled = value });
    }

    public bool? IsBetaVersion
    {
        get => this.appStateStore.Current.IsBetaVersion;
        set => this.appStateStore.Update(
            current => current with { IsBetaVersion = value });
    }

    public string? BetaVersion
    {
        get => this.appStateStore.Current.BetaVersion;
        set => this.appStateStore.Update(
            current => current with { BetaVersion = value });
    }

    public string? RuntimeVersion
    {
        get => this.appStateStore.Current.RuntimeVersion;
        set => this.appStateStore.Update(
            current => current with { RuntimeVersion = value });
    }

    public string? RuntimeReleaseDate
    {
        get => this.appStateStore.Current.RuntimeReleaseDate;
        set => this.appStateStore.Update(
            current => current with { RuntimeReleaseDate = value });
    }

    public string? IdeVersion
    {
        get => this.appStateStore.Current.IdeVersion;
        set => this.appStateStore.Update(
            current => current with { IdeVersion = value });
    }

    public string? IdeReleaseDate
    {
        get => this.appStateStore.Current.IdeReleaseDate;
        set => this.appStateStore.Update(
            current => current with { IdeReleaseDate = value });
    }

    public HwAssemblyType[]? HwAssemblyTypes
    {
        get => this.appStateStore.Current.HwAssemblyTypes;
        set => this.appStateStore.Update(
            current => current with { HwAssemblyTypes = value });
    }

    private static TestEntity[] FindTestEntities(IEnumerable<TestSuiteEntity> testSuites, string[] executionPaths)
    {
        var searchedExecutionPaths = executionPaths.ToHashSet(StringComparer.Ordinal);
        return [.. testSuites
            .AllTestEntities()
            .Where(entity => searchedExecutionPaths.Contains(entity.ExecutionPath))];
    }

    private void OnTestTreeChanged(ExploredTestAssemblyStoreChange change)
    {
        if (change.ExecutionPathsChanged)
        {
            this.SelectedTestEntities = [];
        }
    }
}