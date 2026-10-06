namespace Zat.Tests.Runner.TuiApp.Stores;

using Zat.Tests.Runner.Common.Model;
using Zat.Tests.Runner.Common.Net.Services;
using Zat.Z2xxTests.Common.Model;

/// <summary>
/// This is the store for test run config that is used by the running test.
/// </summary>
/// <remarks>
/// The tests to choose from follow <see cref="ITestTreeStore"/>; once the test assembly holds other test entities,
/// the test selection is cleared, as it may no longer be what the user selected.
/// </remarks>
internal class TestRunConfigStore
{
    private readonly AppStateStore appStateStore;
    private readonly ITestTreeStore testTreeStore;

    public TestRunConfigStore(AppStateStore appStateStore, ITestTreeStore testTreeStore)
    {
        this.appStateStore = appStateStore;
        this.testTreeStore = testTreeStore;

        // Both are singletons, so the subscription lasts as long as the application does.
        this.testTreeStore.Changed += this.OnTestTreeChanged;
    }

    public IEnumerable<TestSuiteEntity> LoadedTestSuites
        => this.testTreeStore.TestSuites;

    public IEnumerable<TestEntity> SelectedTestEntities { get; set; } = [];

    public bool RuntimeTestEntitiesSelected
        => this.SelectedTestEntities.Any(x => x.TestType is TestType.Runtime);

    public bool? IsTestLinkReportingEnabled
    {
        get => this.appStateStore.Current.IsTestLinkReportingEnabled;
        set => this.appStateStore.Update(
            current => current with { IsTestLinkReportingEnabled = value });
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

    private void OnTestTreeChanged(TestTreeChange change)
    {
        if (change.ExecutionPathsChanged)
        {
            this.SelectedTestEntities = [];
        }
    }
}