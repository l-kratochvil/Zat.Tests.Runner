namespace Zat.Tests.Runner.TuiApp.Stores;

using Zat.Tests.Runner.Common.Model;
using Zat.Z2xxTests.Common.Model;

/// <summary>
/// This is the store for test run config that is used by the running test.
/// </summary>
/// <param name="appStateStore"></param>
internal class TestConfigStore(AppStateStore appStateStore)
{
    public IEnumerable<TestSuiteEntity> LoadedTestSuites { get; set; } = [];

    public IEnumerable<TestEntity> SelectedTestEntities { get; set; } = [];

    public bool RuntimeTestEntitiesSelected
        => this.SelectedTestEntities.Any(x => x.TestType is TestType.Runtime);

    public bool? IsTestLinkReportingEnabled
    {
        get => appStateStore.Current.IsTestLinkReportingEnabled;
        set => appStateStore.Update(
            current => current with { IsTestLinkReportingEnabled = value });
    }

    public bool? IsDebugModeEnabled
    {
        get => appStateStore.Current.IsDebugModeEnabled;
        set => appStateStore.Update(
            current => current with { IsDebugModeEnabled = value });
    }

    public bool? IsBetaVersion
    {
        get => appStateStore.Current.IsBetaVersion;
        set => appStateStore.Update(
            current => current with { IsBetaVersion = value });
    }

    public string? BetaVersion
    {
        get => appStateStore.Current.BetaVersion;
        set => appStateStore.Update(
            current => current with { BetaVersion = value });
    }

    public string? RuntimeVersion
    {
        get => appStateStore.Current.RuntimeVersion;
        set => appStateStore.Update(
            current => current with { RuntimeVersion = value });
    }

    public string? RuntimeReleaseDate
    {
        get => appStateStore.Current.RuntimeReleaseDate;
        set => appStateStore.Update(
            current => current with { RuntimeReleaseDate = value });
    }

    public string? IdeVersion
    {
        get => appStateStore.Current.IdeVersion;
        set => appStateStore.Update(
            current => current with { IdeVersion = value });
    }

    public string? IdeReleaseDate
    {
        get => appStateStore.Current.IdeReleaseDate;
        set => appStateStore.Update(
            current => current with { IdeReleaseDate = value });
    }

    public HwAssemblyType[]? HwAssemblyTypes
    {
        get => appStateStore.Current.HwAssemblyTypes;
        set => appStateStore.Update(
            current => current with { HwAssemblyTypes = value });
    }
}