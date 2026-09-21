namespace Zat.Tests.Runner.WebApp.Tests.AppLogging;

using NUnit.Framework;

using Zat.Tests.Runner.WebApp.Features.AppLogging.Components;
using Zat.Tests.Runner.WebApp.Features.AppLogging.Models;
using Zat.Tests.Runner.WebApp.Shared.Logging;

[TestFixture]
public class AppLoggerFilterTests
{
    private AppLoggerFilter unit;

    [SetUp]
    public void SetUp()
    {
        this.unit = AppLoggerFilter.CreateDefault();
    }

    [TestCase(LogSeverity.Info, ExpectedResult = true)]
    [TestCase(LogSeverity.Warning, ExpectedResult = true)]
    [TestCase(LogSeverity.Error, ExpectedResult = true)]
    public bool IsSelected__WhenFilterIsDefault__ThenShouldSelectEverySeverity(
        LogSeverity givenSeverity)
    {
        // When:
        return this.unit.IsSelected(givenSeverity);
    }

    [TestCase(LogSources.App)]
    [TestCase(LogSources.TestRun)]
    [TestCase(LogSources.TestLink)]
    [TestCase("SomeFutureSource")]
    public void IsSelected__WhenFilterIsDefault__ThenShouldSelectEverySourceIncludingUnknownOnes(
        string givenSource)
    {
        // When:
        var result = this.unit.IsSelected(givenSource);

        // Then:
        Assert.That(result, Is.True);
    }

    [TestCase(LogSeverity.Info, LogSources.TestRun, ExpectedResult = true)]
    [TestCase(LogSeverity.Warning, LogSources.App, ExpectedResult = true)]
    [TestCase(LogSeverity.Error, "SomeFutureSource", ExpectedResult = true)]
    public bool Matches__WhenFilterIsDefault__ThenShouldShowEveryEntry(
        LogSeverity givenSeverity, string givenSource)
    {
        // Given:
        var givenEntry = CreateEntry(givenSeverity, givenSource);

        // When:
        return this.unit.Matches(givenEntry);
    }

    [TestCase(LogSeverity.Info)]
    [TestCase(LogSeverity.Warning)]
    [TestCase(LogSeverity.Error)]
    public void Matches__WhenSeverityIsDeselected__ThenShouldHideEntriesOfThatSeverity(
        LogSeverity givenSeverity)
    {
        // Given:
        var givenEntry = CreateEntry(givenSeverity, LogSources.App);
        this.unit.SetSelected(givenSeverity, selected: false);

        // When:
        var result = this.unit.Matches(givenEntry);

        // Then:
        Assert.That(result, Is.False);
    }

    [Test]
    public void Matches__WhenSeverityIsSelectedAgain__ThenShouldShowEntriesOfThatSeverity()
    {
        // Given:
        var givenEntry = CreateEntry(LogSeverity.Info, LogSources.TestRun);
        this.unit.SetSelected(LogSeverity.Info, selected: false);
        this.unit.SetSelected(LogSeverity.Info, selected: true);

        // When:
        var result = this.unit.Matches(givenEntry);

        // Then:
        Assert.That(result, Is.True);
    }

    [TestCase(LogSources.TestRun, LogSources.TestRun, ExpectedResult = false)]
    [TestCase(LogSources.TestRun, LogSources.App, ExpectedResult = true)]
    [TestCase(LogSources.TestLink, LogSources.TestRun, ExpectedResult = true)]
    public bool Matches__WhenSourceIsDeselected__ThenShouldHideOnlyEntriesOfThatSource(
        string givenDeselectedSource, string givenEntrySource)
    {
        // Given:
        var givenEntry = CreateEntry(LogSeverity.Info, givenEntrySource);
        this.unit.SetSelected(givenDeselectedSource, selected: false);

        // When:
        return this.unit.Matches(givenEntry);
    }

    [Test]
    public void Apply__WhenEntriesAreFiltered__ThenShouldKeepTheOriginalOrder()
    {
        // Given:
        string[] expectedMessages = ["first", "second"];
        LogEntry[] givenEntries =
        [
            CreateEntry(LogSeverity.Info, LogSources.App, expectedMessages[0]),
            CreateEntry(LogSeverity.Warning, LogSources.App, "hidden"),
            CreateEntry(LogSeverity.Error, LogSources.App, expectedMessages[1]),
        ];
        this.unit.SetSelected(LogSeverity.Warning, selected: false);

        // When:
        var result = this.unit.Apply(givenEntries);

        // Then:
        Assert.That(result.Select(entry => entry.Message), Is.EqualTo(expectedMessages));
    }

    private static LogEntry CreateEntry(LogSeverity severity, string source, string message = "message")
    {
        return new LogEntry(DateTimeOffset.Now, severity, source, message);
    }
}