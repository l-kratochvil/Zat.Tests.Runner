namespace Zat.Tests.Runner.WebApp.Shared.Stores.TestConfiguration;

using Zat.Z2xxTests.Common.Model;

public record StatusChangedAction(
    ValueChange<bool>? NewHasErrors);

public record DataChangedAction(
    ValueChange<bool>? NewIsTestLinkReportEnabled = null,
    ValueChange<bool>? NewIsBetaVersion = null,
    ValueChange<bool>? NewIsDebugModeEnabled = null,
    ValueChange<HwAssemblyType?>? NewTestedHwAssembly = null,
    ValueChange<Version?>? NewIdeVersion = null,
    ValueChange<string?>? NewRuntimeVersion = null,
    ValueChange<string?>? NewRuntimeReleaseDate = null,
    ValueChange<string?>? NewIdeReleaseDate = null,
    ValueChange<string?>? NewBetaVersion = null);