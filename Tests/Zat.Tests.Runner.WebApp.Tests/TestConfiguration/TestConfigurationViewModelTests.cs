namespace Zat.Tests.Runner.WebApp.Tests.TestConfiguration;

using System.ComponentModel;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;

using Fluxor;

using Microsoft.Extensions.Logging;

using Moq;

using NUnit.Framework;

using Zat.Tests.Runner.Common.Model;
using Zat.Tests.Runner.Common.Net.Services;
using Zat.Tests.Runner.WebApp.Features.TestConfiguration.Components;
using Zat.Tests.Runner.WebApp.Shared.Stores.AppSettings;
using Zat.Tests.Runner.WebApp.Shared.Stores.TestConfiguration;
using Zat.Tests.Runner.WebApp.Shared.Stores.TestDiscovery;
using Zat.Tests.Runner.WebApp.Tests.TestDiscovery;
using Zat.Z2xxTests.Common.Model;

[TestFixture]
public class TestConfigurationViewModelTests
{
    private const string GivenInstallFolderPath = @"C:\Ide";
    private const string MovedInstallFolderPath = @"C:\Ide2";

    private const string GivenTestCasePath = "Suite.Fixture.Test";

    [Test]
    public void WhenStoreStateChanges__ThenShouldUpdate()
    {
        // Given:
        var store = new TestConfigurationStoreFake();
        using var unit = CreateUnit(store: store);

        var state = new TestConfigurationState(
            IsTestLinkReportEnabled: true,
            IsBetaVersion: true,
            IsDebugModeEnabled: true,
            IdeVersion: new Version(6, 1),
            TestedHwAssembly: HwAssemblyType.HW01,
            RuntimeVersion: "6",
            RuntimeReleaseDate: "30.09.2026",
            IdeReleaseDate: "01.10.2026",
            BetaVersion: "3");

        // When:
        store.ChangeTo(state);

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(unit.RuntimeVersion, Is.EqualTo("6"));
            Assert.That(unit.TestedHwAssembly, Is.EqualTo(HwAssemblyType.HW01));
            Assert.That(unit.IsTestLinkReportEnabled, Is.True);
            Assert.That(unit.IdeVersion, Is.EqualTo(new Version(6, 1)));
            Assert.That(unit.IdeReleaseDate, Is.EqualTo("01.10.2026"));
            Assert.That(unit.RuntimeReleaseDate, Is.EqualTo("30.09.2026"));
            Assert.That(unit.IsBetaVersion, Is.True);
            Assert.That(unit.BetaVersion, Is.EqualTo("3"));
            Assert.That(unit.IsDebugModeEnabled, Is.True);
        }
    }

    [Test]
    public void WhenCreatedWithEmptyStoreState__ThenShouldIndicateErrors()
    {
        // Given & When:
        // An empty configuration cannot be run with, so the tester is told so before touching it.
        var store = new TestConfigurationStoreFake();
        using var unit = CreateUnit(store: store);

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(unit.HasErrors, Is.True);
            Assert.That(store.Value.HasErrors, Is.True);
            Assert.That(
                unit.GetValidity(nameof(TestConfigurationViewModel.RuntimeVersion))?.HasErrors,
                Is.True);
        }
    }

    [Test]
    public void RuntimeVersion__WhenSetWithValidValue__ThenShouldIndicateErrors()
    {
        // Given:
        // Neither TestLink nor a runtime test is chosen, so the IDE version and the test station are
        // hidden and nobody could fill them in.
        var store = new TestConfigurationStoreFake();
        using var unit = CreateUnit(store: store);

        // When:
        unit.RuntimeVersion = "6";

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(unit.HasErrors, Is.False);
            Assert.That(store.Value.HasErrors, Is.False);
        }
    }

    [Test]
    public void IsTestLinkReportEnabled__WhenSetWithValidValue__ThenShouldIndicateErrors()
    {
        // Given:
        var store = StoreHolding(new TestConfigurationState { RuntimeVersion = "6" });
        using var unit = CreateUnit(store: store);

        // When:
        unit.IsTestLinkReportEnabled = true;

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(unit.HasErrors, Is.True);
            Assert.That(
                unit.GetValidity(nameof(TestConfigurationViewModel.IdeVersion))?.HasErrors,
                Is.True);
        }
    }

    [TestCaseSource(nameof(PropertySetterCases))]
    public object? Property__WhenSet__ThenShouldHandItToState(
        Action<TestConfigurationViewModel> givenSet,
        Func<TestConfigurationState, object?> readState)
    {
        // Given:
        var store = new TestConfigurationStoreFake();
        using var unit = CreateUnit(store: store);

        // When:
        givenSet(unit);

        // Then:
        return readState(store.Value);
    }

    [TestCaseSource(nameof(RuntimeVersionsCases))]
    public IReadOnlyList<string> RuntimeVersions__WhenInstallFolderHoldsFolders__ThenShouldListVersionsNewestFirst(
        string[] givenFolderNames)
    {
        // Given:
        var fileSystem = new MockFileSystem();
        fileSystem.AddDirectory(GivenInstallFolderPath);
        AddInstalledRuntimeVersions(fileSystem, GivenInstallFolderPath, givenFolderNames);

        // When:
        using var unit = CreateUnit(fileSystem: fileSystem);

        // Then:
        return unit.RuntimeVersions;
    }

    [Test]
    public void RuntimeVersions__WhenInstallFolderCannotBeRead__ThenShouldListNothing()
    {
        // Given & When:
        using var unit = CreateUnit(fileSystem: new MockFileSystem());

        // Then:
        Assert.That(unit.RuntimeVersions, Is.Empty);
    }

    [Test]
    public void RuntimeVersions__WhenInstallFolderCannotBeRead__ThenShouldLogWarning()
    {
        // Given:
        var logger = new Mock<ILogger>();

        // When:
        using var unit = CreateUnit(fileSystem: new MockFileSystem(), logger: logger.Object);

        // Then:
        logger.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [TestCase(true, ExpectedResult = true)]
    [TestCase(false, ExpectedResult = false)]
    public bool IdeVersion__WhenMissing__ThenShouldBeRequiredOnlyForTestLink(bool givenIsTestLinkReportEnabled)
    {
        // Given & When:
        using var unit = CreateUnit(
            store: StoreHolding(new TestConfigurationState
            {
                IsTestLinkReportEnabled = givenIsTestLinkReportEnabled,
            }));

        // Then:
        return HasErrors(unit, nameof(TestConfigurationViewModel.IdeVersion));
    }

    [TestCase("6.1", ExpectedResult = false)]
    [TestCase("6.1.4", ExpectedResult = false)]
    [TestCase("6.1.4.2", ExpectedResult = true)]
    public bool IdeVersion__WhenTestLinkIsEnabled__ThenShouldAcceptOnlyTwoOrThreeParts(string givenIdeVersion)
    {
        // Given & When:
        using var unit = CreateUnit(
            store: StoreHolding(new TestConfigurationState
            {
                IsTestLinkReportEnabled = true,
                IdeVersion = Version.Parse(givenIdeVersion),
            }));

        // Then:
        return HasErrors(unit, nameof(TestConfigurationViewModel.IdeVersion));
    }

    [TestCase(null, ExpectedResult = true)]
    [TestCase(HwAssemblyType.Unknown, ExpectedResult = true)]
    [TestCase(HwAssemblyType.HW01, ExpectedResult = false)]
    public bool TestedHwAssembly__WhenRuntimeTestIsSelected__ThenShouldRequireKnownStation(
        HwAssemblyType? givenTestedHwAssembly)
    {
        // Given & When:
        using var unit = CreateUnit(
            store: StoreHolding(new TestConfigurationState { TestedHwAssembly = givenTestedHwAssembly }),
            testSelectionState: SelectionOf(GivenTestCasePath),
            exploredTestAssemblyStore: TreeWithOneTestCase(TestType.Runtime));

        // Then:
        return HasErrors(unit, nameof(TestConfigurationViewModel.TestedHwAssembly));
    }

    [Test]
    public void TestedHwAssembly__WhenNoRuntimeTestIsSelected__ThenShouldNotBeRequired()
    {
        // Given & When:
        using var unit = CreateUnit(
            testSelectionState: SelectionOf(GivenTestCasePath),
            exploredTestAssemblyStore: TreeWithOneTestCase(TestType.Application));

        // Then:
        Assert.That(HasErrors(unit, nameof(TestConfigurationViewModel.TestedHwAssembly)), Is.False);
    }

    [TestCase("1.10.2026", ExpectedResult = false)]
    [TestCase("01.10.2026", ExpectedResult = false)]
    [TestCase("", ExpectedResult = false)]
    [TestCase("2026-10-01", ExpectedResult = true)]
    [TestCase("31.2.2026", ExpectedResult = true)]
    public bool IdeReleaseDate__WhenTestLinkIsEnabled__ThenShouldAcceptOnlyDate(string givenDate)
    {
        // Given & When:
        using var unit = CreateUnit(
            store: StoreHolding(new TestConfigurationState
            {
                IsTestLinkReportEnabled = true,
                IdeReleaseDate = givenDate,
            }));

        // Then:
        return HasErrors(unit, nameof(TestConfigurationViewModel.IdeReleaseDate));
    }

    [TestCase("1.10.2026", ExpectedResult = false)]
    [TestCase("01.10.2026", ExpectedResult = false)]
    [TestCase("", ExpectedResult = false)]
    [TestCase("2026-10-01", ExpectedResult = true)]
    [TestCase("31.2.2026", ExpectedResult = true)]
    public bool RuntimeReleaseDate__WhenTestLinkIsEnabled__ThenShouldAcceptOnlyDate(string givenDate)
    {
        // Given & When:
        using var unit = CreateUnit(
            store: StoreHolding(new TestConfigurationState
            {
                IsTestLinkReportEnabled = true,
                RuntimeReleaseDate = givenDate,
            }));

        // Then:
        return HasErrors(unit, nameof(TestConfigurationViewModel.RuntimeReleaseDate));
    }

    [Test]
    public void ReleaseDates__WhenTestLinkIsDisabled__ThenShouldIgnoreThem()
    {
        // Given & When:
        using var unit = CreateUnit(
            store: StoreHolding(new TestConfigurationState
            {
                IdeReleaseDate = "nonsense",
                RuntimeReleaseDate = "nonsense",
            }));

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(HasErrors(unit, nameof(TestConfigurationViewModel.IdeReleaseDate)), Is.False);
            Assert.That(HasErrors(unit, nameof(TestConfigurationViewModel.RuntimeReleaseDate)), Is.False);
        }
    }

    [TestCase("3", ExpectedResult = false)]
    [TestCase("", ExpectedResult = false)]
    [TestCase("three", ExpectedResult = true)]
    [TestCase("3.1", ExpectedResult = true)]
    public bool BetaVersion__WhenBuildIsBeta__ThenShouldAcceptOnlyWholeNumber(string givenBetaVersion)
    {
        // Given & When:
        using var unit = CreateUnit(
            store: StoreHolding(new TestConfigurationState
            {
                IsTestLinkReportEnabled = true,
                IsBetaVersion = true,
                BetaVersion = givenBetaVersion,
            }));

        // Then:
        return HasErrors(unit, nameof(TestConfigurationViewModel.BetaVersion));
    }

    [Test]
    public void BetaVersion__WhenBuildIsNotBeta__ThenShouldIgnoreIt()
    {
        // Given & When:
        using var unit = CreateUnit(
            store: StoreHolding(new TestConfigurationState
            {
                IsTestLinkReportEnabled = true,
                BetaVersion = "three",
            }));

        // Then:
        Assert.That(HasErrors(unit, nameof(TestConfigurationViewModel.BetaVersion)), Is.False);
    }

    [TestCase(true, ExpectedResult = true)]
    [TestCase(false, ExpectedResult = false)]
    public bool EmailAddresses__WhenAddressIsInvalid__ThenShouldIndicateErrorsOnlyWhenEmailed(
        bool givenIsEmailNotificationEnabled)
    {
        // Given & When:
        var store = StoreHolding(new TestConfigurationState
        {
            RuntimeVersion = "6",
            IsEmailNotificationEnabled = givenIsEmailNotificationEnabled,
            EmailAddresses = ["someone@example.com"],
        });
        using var unit = CreateUnit(store: store);

        // Then:
        Assert.That(store.Value.HasErrors, Is.EqualTo(unit.HasErrors));
        return unit.HasErrors;
    }

    [Test]
    public void EmailAddresses__WhenStoreStateChangesToInvalidAddress__ThenShouldIndicateErrors()
    {
        // Given:
        var store = StoreHolding(new TestConfigurationState
        {
            RuntimeVersion = "6",
            IsEmailNotificationEnabled = true,
        });
        using var unit = CreateUnit(store: store);

        // When:
        store.ChangeTo(store.Value with { EmailAddresses = ["someone@example.com"] });

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(unit.HasErrors, Is.True);
            Assert.That(
                unit.EmailAddressesEditor.GetValidity(nameof(EmailAddressesEditorViewModel.EmailAddresses))?.HasErrors,
                Is.True);
        }
    }

    [Test]
    public void EmailAddressesEditor__WhenAddressBeingTypedIsInvalid__ThenShouldNotIndicateErrors()
    {
        // Given:
        // An address not confirmed yet is not part of the configuration, so it must not stop a run.
        using var unit = CreateUnit(
            store: StoreHolding(new TestConfigurationState
            {
                RuntimeVersion = "6",
                IsEmailNotificationEnabled = true,
            }));
        _ = unit.EmailAddressesEditor.BeginAdd();

        // When:
        unit.EmailAddressesEditor.EmailAddress = "someone@example.com";

        // Then:
        Assert.That(unit.HasErrors, Is.False);
    }

    [TestCase(TestType.Runtime, ExpectedResult = true)]
    [TestCase(TestType.Application, ExpectedResult = false)]
    public bool IsRuntimeTest__WhenTestIsSelected__ThenShouldTellWhetherItIsRuntimeTest(TestType givenTestType)
    {
        // Given:
        using var unit = CreateUnit(
            testSelectionState: SelectionOf(GivenTestCasePath),
            exploredTestAssemblyStore: TreeWithOneTestCase(givenTestType));

        // When:
        return unit.IsRuntimeTest;
    }

    [Test]
    public void OnTestSelectionChanged__WhenSelectionChanges__ThenShouldAnnounceIsRuntimeTest()
    {
        // Given:
        var testSelectionState = new Mock<IState<TestDiscoveryState>>();
        testSelectionState.SetupGet(state => state.Value).Returns(new TestDiscoveryState());
        using var unit = CreateUnit(testSelectionState: testSelectionState.Object);
        var announced = RecordAnnouncements(unit);

        // When:
        testSelectionState.Raise(state => state.StateChanged += null, EventArgs.Empty);

        // Then:
        Assert.That(announced, Does.Contain(nameof(TestConfigurationViewModel.IsRuntimeTest)));
    }

    [Test]
    public void OnAppSettingsChanged__WhenInstallFolderMoves__ThenShouldListRuntimeVersionsInstalledThere()
    {
        // Given:
        var fileSystem = new MockFileSystem();
        AddInstalledRuntimeVersions(fileSystem, GivenInstallFolderPath, "7", "6");
        AddInstalledRuntimeVersions(fileSystem, MovedInstallFolderPath, "9");
        var appSettingsStore = AppSettingsStoreAt(GivenInstallFolderPath);
        using var unit = CreateUnit(appSettingsStore: appSettingsStore.Object, fileSystem: fileSystem);

        // When:
        RaiseAppSettingsChanged(appSettingsStore, MovedInstallFolderPath);

        // Then:
        Assert.That(unit.RuntimeVersions, Is.EqualTo(["9"]));
    }

    [Test]
    public void OnAppSettingsChanged__WhenInstallFolderMoves__ThenShouldAnnounceRuntimeVersions()
    {
        // Given:
        var appSettingsStore = AppSettingsStoreAt(GivenInstallFolderPath);
        using var unit = CreateUnit(appSettingsStore: appSettingsStore.Object);
        var announced = RecordAnnouncements(unit);

        // When:
        RaiseAppSettingsChanged(appSettingsStore, MovedInstallFolderPath);

        // Then:
        Assert.That(announced, Does.Contain(nameof(TestConfigurationViewModel.RuntimeVersions)));
    }

    [Test]
    public void OnAppSettingsChanged__WhenChosenVersionIsNotInstalledThere__ThenShouldClearIt()
    {
        // Given:
        var fileSystem = new MockFileSystem();
        AddInstalledRuntimeVersions(fileSystem, GivenInstallFolderPath, "7", "6");
        AddInstalledRuntimeVersions(fileSystem, MovedInstallFolderPath, "9");
        var appSettingsStore = AppSettingsStoreAt(GivenInstallFolderPath);
        using var unit = CreateUnit(
            store: StoreHolding(new TestConfigurationState { RuntimeVersion = "6" }),
            appSettingsStore: appSettingsStore.Object,
            fileSystem: fileSystem);

        // When:
        RaiseAppSettingsChanged(appSettingsStore, MovedInstallFolderPath);

        // Then:
        Assert.That(unit.RuntimeVersion, Is.Null);
    }

    [Test]
    public void OnAppSettingsChanged__WhenChosenVersionIsInstalledThereToo__ThenShouldKeepIt()
    {
        // Given:
        var fileSystem = new MockFileSystem();
        AddInstalledRuntimeVersions(fileSystem, GivenInstallFolderPath, "7", "6");
        AddInstalledRuntimeVersions(fileSystem, MovedInstallFolderPath, "6");
        var appSettingsStore = AppSettingsStoreAt(GivenInstallFolderPath);
        using var unit = CreateUnit(
            store: StoreHolding(new TestConfigurationState { RuntimeVersion = "6" }),
            appSettingsStore: appSettingsStore.Object,
            fileSystem: fileSystem);

        // When:
        RaiseAppSettingsChanged(appSettingsStore, MovedInstallFolderPath);

        // Then:
        Assert.That(unit.RuntimeVersion, Is.EqualTo("6"));
    }

    [Test]
    public void Dispose__WhenAppSettingsChangeAfterwards__ThenShouldIgnoreThem()
    {
        // Given:
        var appSettingsStore = AppSettingsStoreAt(GivenInstallFolderPath);
        var unit = CreateUnit(appSettingsStore: appSettingsStore.Object);
        var announced = RecordAnnouncements(unit);

        // When:
        unit.Dispose();
        RaiseAppSettingsChanged(appSettingsStore, MovedInstallFolderPath);

        // Then:
        Assert.That(announced, Is.Empty);
    }

    [Test]
    public void Dispose__WhenTestSelectionChangesAfterwards__ThenShouldIgnoreIt()
    {
        // Given:
        var testSelectionState = new Mock<IState<TestDiscoveryState>>();
        testSelectionState.SetupGet(state => state.Value).Returns(new TestDiscoveryState());
        var unit = CreateUnit(testSelectionState: testSelectionState.Object);
        var announced = RecordAnnouncements(unit);

        // When:
        unit.Dispose();
        testSelectionState.Raise(state => state.StateChanged += null, EventArgs.Empty);

        // Then:
        Assert.That(announced, Is.Empty);
    }

    [Test]
    public void Dispose__WhenStoreStateChangesAfterwards__ThenShouldIgnoreIt()
    {
        // Given:
        var store = new TestConfigurationStoreFake();
        var unit = CreateUnit(store: store);
        var announced = RecordAnnouncements(unit);

        // When:
        unit.Dispose();
        store.ChangeTo(new TestConfigurationState { RuntimeVersion = "6" });

        // Then:
        Assert.That(announced, Is.Empty);
    }

    private static IEnumerable<TestCaseData> PropertySetterCases()
    {
        yield return SetterCase(
            nameof(TestConfigurationViewModel.RuntimeVersion),
            vm => vm.RuntimeVersion = "6",
            state => state.RuntimeVersion,
            "6");

        yield return SetterCase(
            nameof(TestConfigurationViewModel.IdeVersion),
            vm => vm.IdeVersion = new Version(6, 1),
            state => state.IdeVersion,
            new Version(6, 1));

        yield return SetterCase(
            nameof(TestConfigurationViewModel.TestedHwAssembly),
            vm => vm.TestedHwAssembly = HwAssemblyType.HW01,
            state => state.TestedHwAssembly,
            HwAssemblyType.HW01);

        yield return SetterCase(
            nameof(TestConfigurationViewModel.IsTestLinkReportEnabled),
            vm => vm.IsTestLinkReportEnabled = true,
            state => state.IsTestLinkReportEnabled,
            true);

        yield return SetterCase(
            nameof(TestConfigurationViewModel.IdeReleaseDate),
            vm => vm.IdeReleaseDate = "1.10.2026",
            state => state.IdeReleaseDate,
            "1.10.2026");

        yield return SetterCase(
            nameof(TestConfigurationViewModel.RuntimeReleaseDate),
            vm => vm.RuntimeReleaseDate = "30.9.2026",
            state => state.RuntimeReleaseDate,
            "30.9.2026");

        yield return SetterCase(
            nameof(TestConfigurationViewModel.IsBetaVersion),
            vm => vm.IsBetaVersion = true,
            state => state.IsBetaVersion,
            true);

        yield return SetterCase(
            nameof(TestConfigurationViewModel.BetaVersion),
            vm => vm.BetaVersion = "3",
            state => state.BetaVersion,
            "3");

        yield return SetterCase(
            nameof(TestConfigurationViewModel.IsDebugModeEnabled),
            vm => vm.IsDebugModeEnabled = true,
            state => state.IsDebugModeEnabled,
            true);
    }

    private static TestCaseData SetterCase(
        string prefix,
        Action<TestConfigurationViewModel> set,
        Func<TestConfigurationState, object?> readState,
        object expected)
        => new TestCaseData(set, readState)
            .SetName(prefix + "__WhenSet__ThenShouldHandItToState")
            .Returns(expected);

    private static IEnumerable<TestCaseData> RuntimeVersionsCases()
    {
        const string prefix = nameof(TestConfigurationViewModel.RuntimeVersions);

        yield return new TestCaseData([new[] { "9", "10", "6" }])
            .SetName(prefix + "__WhenVersionsAreNumbers__ThenShouldOrderThemByValue")
            .Returns(new[] { "10", "9", "6" });

        yield return new TestCaseData([new[] { "7", "Tools", "6" }])
            .SetName(prefix + "__WhenFolderIsNotVersion__ThenShouldLeaveItOut")
            .Returns(new[] { "7", "6" });

        yield return new TestCaseData([Array.Empty<string>()])
            .SetName(prefix + "__WhenInstallFolderIsEmpty__ThenShouldListNothing")
            .Returns(Array.Empty<string>());
    }

    private static TestConfigurationViewModel CreateUnit(
        TestConfigurationStoreFake? store = null,
        IAppSettingsStore? appSettingsStore = null,
        IFileSystem? fileSystem = null,
        IState<TestDiscoveryState>? testSelectionState = null,
        IExploredTestAssemblyStore? exploredTestAssemblyStore = null,
        ILogger? logger = null)
    {
        // The store fake rather than a mock: what the view model shows is the rules applied to what
        // is being typed, which only a state that takes what is dispatched can show.
        store ??= new TestConfigurationStoreFake();

        return new TestConfigurationViewModel(
            store,
            appSettingsStore ?? AppSettingsStoreAt(GivenInstallFolderPath).Object,
            fileSystem ?? new MockFileSystem(),
            testSelectionState ?? SelectionOf(),
            exploredTestAssemblyStore ?? Mock.Of<IExploredTestAssemblyStore>(s => s.TestSuites == Array.Empty<TestSuiteEntity>()),
            logger ?? Mock.Of<ILogger>(),
            store.Dispatcher.Object);
    }

    private static TestConfigurationStoreFake StoreHolding(TestConfigurationState state)
        => new() { Value = state };

    private static Mock<IAppSettingsStore> AppSettingsStoreAt(string installFolderPath)
    {
        var appSettingsStore = new Mock<IAppSettingsStore>();
        appSettingsStore
            .SetupGet(store => store.Current)
            .Returns(new AppSettingsState(installFolderPath));

        return appSettingsStore;
    }

    private static void RaiseAppSettingsChanged(Mock<IAppSettingsStore> appSettingsStore, string newInstallFolderPath)
    {
        var settings = new AppSettingsState(newInstallFolderPath);
        appSettingsStore.SetupGet(store => store.Current).Returns(settings);
        appSettingsStore.Raise(store => store.Changed += null, settings);
    }

    private static IState<TestDiscoveryState> SelectionOf(params string[] executionPaths)
        => Mock.Of<IState<TestDiscoveryState>>(s => s.Value == new TestDiscoveryState(executionPaths));

    private static IExploredTestAssemblyStore TreeWithOneTestCase(TestType testType)
        => Mock.Of<IExploredTestAssemblyStore>(
            s => s.TestSuites == new[] { TestSuites.WithOneTestCase(testType, GivenTestCasePath) });

    private static void AddInstalledRuntimeVersions(
        MockFileSystem fileSystem,
        string installFolderPath,
        params string[] versions)
    {
        foreach (var version in versions)
        {
            fileSystem.AddDirectory(fileSystem.Path.Combine(installFolderPath, version));
        }
    }

    private static bool HasErrors(TestConfigurationViewModel unit, string propertyName)
        => unit.GetValidity(propertyName)?.HasErrors ?? false;

    private static List<string?> RecordAnnouncements(INotifyPropertyChanged unit)
    {
        var announced = new List<string?>();
        unit.PropertyChanged += (_, e) => announced.Add(e.PropertyName);

        return announced;
    }
}