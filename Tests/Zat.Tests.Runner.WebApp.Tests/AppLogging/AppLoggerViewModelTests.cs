namespace Zat.Tests.Runner.WebApp.Tests.AppLogging;

using Fluxor;

using Microsoft.Extensions.Logging;

using Moq;

using NUnit.Framework;

using Zat.Tests.Runner.Common.Net.Application.Logging;
using Zat.Tests.Runner.WebApp.Features.AppLogging.Components;
using Zat.Tests.Runner.WebApp.Shared.Model;
using Zat.Tests.Runner.WebApp.Shared.Stores.AppLogging;

[TestFixture]
public class AppLoggerViewModelTests
{
    private const string GivenUnknownSource = "Unknown source";

    private Mock<IState<AppLoggingState>> state;
    private AppLoggingState log;

    private AppLoggerViewModel unit;

    [SetUp]
    public void SetUp()
    {
        this.log = new AppLoggingState();

        this.state = new Mock<IState<AppLoggingState>>();
        this.state.SetupGet(state => state.Value).Returns(() => this.log);

        this.unit = new AppLoggerViewModel(this.state.Object);
    }

    [TearDown]
    public void TearDown()
        => this.unit.Dispose();

    [Test]
    public void ErrorCount__WhenLogHoldsErrors__ThenShouldCountThemWhateverFilterHides()
    {
        // Given:
        // The count stands in for the log while the panel is collapsed, so hiding errors from the
        // list must not hide them from the count.
        this.GivenLog(
            CreateEntry(LogLevel.Error),
            CreateEntry(LogLevel.Warning),
            CreateEntry(LogLevel.Error));
        this.unit.SetSelected(LogLevel.Error, selected: false);

        // When:
        var errorCount = this.unit.ErrorCount;

        // Then:
        Assert.That(errorCount, Is.EqualTo(2));
    }

    [Test]
    public void Entries__WhenLogLevelIsDeselected__ThenShouldLeaveItsEntriesOut()
    {
        // Given:
        var expected = CreateEntry(LogLevel.Warning);
        this.GivenLog(CreateEntry(LogLevel.Information), expected);

        // When:
        this.unit.SetSelected(LogLevel.Information, selected: false);

        // Then:
        Assert.That(this.unit.Entries, Is.EqualTo(new[] { expected }));
    }

    [Test]
    public void Entries__WhenSourceIsDeselected__ThenShouldLeaveItsEntriesOut()
    {
        // Given:
        var expected = CreateEntry(LogLevel.Information, LogSources.All[0]);
        this.GivenLog(CreateEntry(LogLevel.Information, GivenUnknownSource), expected);

        // When:
        this.unit.SetSelected(GivenUnknownSource, selected: false);

        // Then:
        Assert.That(this.unit.Entries, Is.EqualTo(new[] { expected }));
    }

    [Test]
    public void SetSelected__WhenFilterChanges__ThenShouldAnnounceEntries()
    {
        // Given:
        var announced = this.RecordAnnouncedProperties();

        // When:
        this.unit.SetSelected(LogLevel.Debug, selected: false);

        // Then:
        Assert.That(announced, Does.Contain(nameof(AppLoggerViewModel.Entries)));
    }

    [Test]
    public void AvailableSources__WhenLogHeardFromUnknownSource__ThenShouldOfferItBesideKnownOnes()
    {
        // Given:
        // A source outside the known ones would otherwise be impossible to hide.
        string[] expected = [..LogSources.All, GivenUnknownSource];
        this.GivenLog(
            CreateEntry(LogLevel.Information, GivenUnknownSource),
            CreateEntry(LogLevel.Information, GivenUnknownSource.ToUpperInvariant()));

        // When:
        var sources = this.unit.AvailableSources;

        // Then:
        Assert.That(sources, Is.EqualTo(expected));
    }

    [Test]
    public void ToggleCollapsed__WhenPanelIsOpen__ThenShouldCollapseItAndAnnounceIt()
    {
        // Given:
        var announced = this.RecordAnnouncedProperties();

        // When:
        this.unit.ToggleCollapsed();

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(this.unit.IsCollapsed, Is.True);
            Assert.That(announced, Does.Contain(nameof(AppLoggerViewModel.IsCollapsed)));
        }
    }

    [Test]
    public void OnStateChanged__WhenEntryIsLogged__ThenShouldAnnounceWhatIsDrawnFromLog()
    {
        // Given:
        string[] expected =
        [
            nameof(AppLoggerViewModel.Entries),
            nameof(AppLoggerViewModel.ErrorCount),
            nameof(AppLoggerViewModel.AvailableSources),
        ];
        var announced = this.RecordAnnouncedProperties();

        // When:
        this.ChangeLog(CreateEntry(LogLevel.Error));

        // Then:
        Assert.That(announced, Is.SupersetOf(expected));
    }

    [Test]
    public void Dispose__WhenEntryIsLoggedAfterwards__ThenShouldAnnounceNothing()
    {
        // Given:
        var announced = this.RecordAnnouncedProperties();

        // When:
        this.unit.Dispose();
        this.ChangeLog(CreateEntry(LogLevel.Error));

        // Then:
        Assert.That(announced, Is.Empty);
    }

    private static LogEntry CreateEntry(LogLevel logLevel, string? source = null)
        => new(
            Timestamp: DateTimeOffset.Now,
            LogLevel: logLevel,
            Source: source ?? LogSources.All[0],
            Message: logLevel.ToString());

    private void GivenLog(params LogEntry[] entries)
        => this.log = new AppLoggingState { Entries = entries };

    private void ChangeLog(params LogEntry[] entries)
    {
        this.GivenLog(entries);
        this.state.Raise(state => state.StateChanged += null, EventArgs.Empty);
    }

    private List<string?> RecordAnnouncedProperties()
    {
        var announced = new List<string?>();
        this.unit.PropertyChanged += (_, e) => announced.Add(e.PropertyName);
        return announced;
    }
}