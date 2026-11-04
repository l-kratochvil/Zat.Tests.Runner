namespace Zat.Tests.Runner.WebApp.Tests.TestConfiguration;

using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;

using Bunit;

using Fluxor;

using Microsoft.Extensions.DependencyInjection;

using Moq;

using NUnit.Framework;

using Zat.Tests.Runner.Common.Model;
using Zat.Tests.Runner.Common.Net.Services;
using Zat.Tests.Runner.WebApp.Features.TestConfiguration.Components;
using Zat.Tests.Runner.WebApp.Shared.Stores.AppSettings;
using Zat.Tests.Runner.WebApp.Shared.Stores.TestConfiguration;
using Zat.Tests.Runner.WebApp.Shared.Stores.TestDiscovery;
using Zat.Tests.Runner.WebApp.Tests.Application.Logging;
using Zat.Tests.Runner.WebApp.Tests.TestDiscovery;

using TestConfigurationComponent = Zat.Tests.Runner.WebApp.Features.TestConfiguration.Components.TestConfiguration;

/// <summary>
/// Which fields the configurator shows, which is the part of it that is not simply bound to a field.
/// </summary>
/// <remarks>
/// Only the rules about which fields appear at all are exercised here; what each field does with
/// what is typed into it belongs to <see cref="TestConfigurationViewModelTests"/>, which can say it
/// without rendering anything.
/// </remarks>
[TestFixture]
public class TestConfigurationTests
{
    private const string RuntimeVersionSelector = ".input-runtime-version";
    private const string TestStationSelector = ".input-hw-assembly-type";
    private const string TestLinkSelector = ".input-report-to-testlink";
    private const string IdeVersionSelector = ".input-ide-version";
    private const string IdeReleaseDateSelector = ".input-ide-release-date";
    private const string RuntimeReleaseDateSelector = ".input-runtime-release-date";
    private const string BetaSelector = ".input-is-beta-version";
    private const string BetaNumberSelector = ".input-beta-version";
    private const string EmailSelector = ".input-report-by-email";
    private const string DebugModeSelector = ".input-debug-mode";
    private const string ErrorMessageSelector = ".validity-message.is-error";

    // The choice offered in a select for none of its options.
    private const string NoChoiceLabel = "—";

    private const string GivenTestCasePath = "Suite.Fixture.Test";

    private const string GivenInstallFolderPath = @"C:\Ide";
    private const string MovedInstallFolderPath = @"C:\Ide2";

    [Test]
    public void Render__WhenConfiguratorIsShown__ThenShouldListInstalledRuntimeVersions()
    {
        // Given:
        var fileSystem = new MockFileSystem();
        AddInstalledRuntimeVersions(fileSystem, GivenInstallFolderPath, "7", "6");

        // When:
        using var context = CreateUnit(out var unit, fileSystem: fileSystem);

        // Then:
        // The versions plus the empty choice standing for none of them.
        Assert.That(unit.FindAll($"{RuntimeVersionSelector} option"), Has.Exactly(3).Items);
    }

    [Test]
    public void Render__WhenNoRuntimeTestIsSelected__ThenShouldHideTestStation()
    {
        // Given:
        // Only a runtime test runs against hardware, so for anything else the field is not shown at
        // all rather than shown and ignored.
        var testSelectionState = SelectionOf(GivenTestCasePath);
        var testTreeStore = TreeWithOneTestCase(TestType.Application);

        // When:
        using var context = CreateUnit(
            out var unit,
            testSelectionState: testSelectionState,
            testTreeStore: testTreeStore);

        // Then:
        Assert.That(unit.FindAll(TestStationSelector), Is.Empty);
    }

    [Test]
    public void Render__WhenRuntimeTestIsSelected__ThenShouldShowTestStation()
    {
        // Given:
        var testSelectionState = SelectionOf(GivenTestCasePath);
        var testTreeStore = TreeWithOneTestCase(TestType.Runtime);

        // When:
        using var context = CreateUnit(
            out var unit,
            testSelectionState: testSelectionState,
            testTreeStore: testTreeStore);

        // Then:
        Assert.That(unit.FindAll(TestStationSelector), Has.Exactly(1).Items);
    }

    [Test]
    public void OnTestSelectionChanged__WhenRuntimeTestGetsSelected__ThenShouldShowTestStation()
    {
        // Given:
        var testSelection = new TestDiscoveryState();
        var testSelectionState = new Mock<IState<TestDiscoveryState>>();
        testSelectionState.SetupGet(state => state.Value).Returns(() => testSelection);

        using var context = CreateUnit(
            out var unit,
            testSelectionState: testSelectionState.Object,
            testTreeStore: TreeWithOneTestCase(TestType.Runtime));

        // When:
        testSelection = new TestDiscoveryState([GivenTestCasePath]);
        testSelectionState.Raise(state => state.StateChanged += null, EventArgs.Empty);

        // Then:
        unit.WaitForAssertion(() => Assert.That(unit.FindAll(TestStationSelector), Has.Exactly(1).Items));
    }

    [Test]
    public void Render__WhenResultDoesNotGoToTestLink__ThenShouldHideIdeVersion()
    {
        // Given & When:
        using var context = CreateUnit(out var unit);

        // Then:
        Assert.That(unit.FindAll(IdeVersionSelector), Is.Empty);
    }

    [Test]
    public void Render__WhenResultDoesNotGoToTestLink__ThenShouldShowOnlyDebugMode()
    {
        // Given & When:
        using var context = CreateUnit(out var unit);

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(unit.FindAll(IdeReleaseDateSelector), Is.Empty);
            Assert.That(unit.FindAll(RuntimeReleaseDateSelector), Is.Empty);
            Assert.That(unit.FindAll(BetaSelector), Is.Empty);
            Assert.That(unit.FindAll(BetaNumberSelector), Is.Empty);
            Assert.That(unit.FindAll(DebugModeSelector), Has.Exactly(1).Items);
        }
    }

    [Test]
    public void OnTestLinkEnabled__WhenResultIsToGoToTestLink__ThenShouldShowReleaseDatesAndBetaCheckbox()
    {
        // Given:
        using var context = CreateUnit(out var unit);

        // When:
        unit.Find(TestLinkSelector).Change(true);

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(unit.FindAll(IdeReleaseDateSelector), Has.Exactly(1).Items);
            Assert.That(unit.FindAll(RuntimeReleaseDateSelector), Has.Exactly(1).Items);
            Assert.That(unit.FindAll(BetaSelector), Has.Exactly(1).Items);
            Assert.That(unit.FindAll(BetaNumberSelector), Is.Empty);
        }
    }

    [Test]
    public void OnBetaEnabled__WhenTestedBuildIsBeta__ThenShouldShowBetaNumber()
    {
        // Given:
        using var context = CreateUnit(out var unit);
        unit.Find(TestLinkSelector).Change(true);

        // When:
        unit.Find(BetaSelector).Change(true);

        // Then:
        Assert.That(unit.FindAll(BetaNumberSelector), Has.Exactly(1).Items);
    }

    [Test]
    public void OnTestLinkEnabled__WhenResultIsToGoToTestLink__ThenShouldShowIdeVersion()
    {
        // Given:
        // The version is what the result is filed under, so it is shown exactly when there is
        // somewhere to file it.
        using var context = CreateUnit(out var unit);

        // When:
        unit.Find(TestLinkSelector).Change(true);

        // Then:
        Assert.That(unit.FindAll(IdeVersionSelector), Has.Exactly(1).Items);
    }

    [Test]
    public void OnTestLinkDisabled__WhenResultIsNotToGoToTestLinkAfterAll__ThenShouldHideIdeVersion()
    {
        // Given:
        using var context = CreateUnit(out var unit);
        unit.Find(TestLinkSelector).Change(true);

        // When:
        unit.Find(TestLinkSelector).Change(false);

        // Then:
        Assert.That(unit.FindAll(IdeVersionSelector), Is.Empty);
    }

    [Test]
    public void OnEmailNotificationEnabled__WhenResultIsToGoByEmail__ThenShouldRender()
    {
        // Given:
        using var dependencies = CreateUnit(out var unit);

        // When & Then:
        Assert.DoesNotThrow(() => unit.Find(EmailSelector).Change(true));
    }

    [Test]
    public void OnIdeVersionTyped__WhenWhatWasTypedIsNotVersion__ThenShouldDisplayError()
    {
        // Given:
        using var context = CreateUnit(out var unit);
        unit.Find(TestLinkSelector).Change(true);

        // When:
        unit.Find(IdeVersionSelector).Change("nonsense");

        // Then:
        Assert.That(unit.FindAll(ErrorMessageSelector), Is.Not.Empty);
    }

    [Test]
    public void OnAppSettingsChanged__WhenInstallFolderMoves__ThenShouldListRuntimeVersionsInstalledThere()
    {
        // Given:
        var fileSystem = new MockFileSystem();
        AddInstalledRuntimeVersions(fileSystem, GivenInstallFolderPath, "7", "6");
        AddInstalledRuntimeVersions(fileSystem, MovedInstallFolderPath, "9");

        var appSettingsStore = new Mock<IAppSettingsStore>();
        appSettingsStore
            .SetupGet(store => store.Current)
            .Returns(new AppSettingsState(GivenInstallFolderPath));

        using var context = CreateUnit(
            out var unit,
            appSettingsStore: appSettingsStore.Object,
            fileSystem: fileSystem);

        // When:
        // The settings are one instance shared by everyone connected, so they announce a change on
        // the thread of whoever made it rather than on this circuit's.
        var settings = new AppSettingsState(MovedInstallFolderPath);
        appSettingsStore.SetupGet(store => store.Current).Returns(settings);
        appSettingsStore.Raise(store => store.Changed += null, settings);

        // Then:
        unit.WaitForAssertion(() => Assert.That(
            unit.FindAll($"{RuntimeVersionSelector} option").Select(option => option.TextContent.Trim()),
            Is.EqualTo([NoChoiceLabel, "9"])));
    }

    /// <summary>
    /// Renders the configurator in a test context of its own, which the returned disposer disposes.
    /// </summary>
    private static IDisposable CreateUnit(
        out IRenderedComponent<TestConfigurationComponent> unit,
        TestConfigurationStoreFake? store = null,
        IAppSettingsStore? appSettingsStore = null,
        IFileSystem? fileSystem = null,
        IState<TestDiscoveryState>? testSelectionState = null,
        ITestTreeStore? testTreeStore = null)
    {
        // The store fake rather than a mock, so that a field switched on is shown switched on.
        store ??= new TestConfigurationStoreFake();

        var context = new Bunit.TestContext();
        context.Services.AddSingleton<IState<TestConfigurationState>>(store);
        context.Services.AddSingleton(store.Dispatcher.Object);
        context.Services.AddSingleton(
            appSettingsStore
            ?? Mock.Of<IAppSettingsStore>(s => s.Current == new AppSettingsState(GivenInstallFolderPath)));
        context.Services.AddSingleton(fileSystem ?? new MockFileSystem());
        context.Services.AddSingleton(testSelectionState ?? SelectionOf());
        context.Services.AddSingleton(
            testTreeStore
            ?? Mock.Of<ITestTreeStore>(s => s.TestSuites == Array.Empty<TestSuiteEntity>()));
        context.Services.AddSingleton(Mock.Of<IActionSubscriber>());
        context.Services.InitLoggingAsApp();
        context.Services.AddScoped<TestConfigurationViewModel>();

        unit = context.RenderComponent<TestConfigurationComponent>();

        return context;
    }

    private static IState<TestDiscoveryState> SelectionOf(params string[] executionPaths)
        => Mock.Of<IState<TestDiscoveryState>>(s => s.Value == new TestDiscoveryState(executionPaths));

    private static ITestTreeStore TreeWithOneTestCase(TestType testType)
        => Mock.Of<ITestTreeStore>(
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
}