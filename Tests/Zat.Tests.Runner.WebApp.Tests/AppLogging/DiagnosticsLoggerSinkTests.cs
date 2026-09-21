namespace Zat.Tests.Runner.WebApp.Tests.AppLogging;

using Microsoft.Extensions.Logging;

using Moq;

using NUnit.Framework;

using Zat.Tests.Runner.WebApp.Features.AppLogging.Models;
using Zat.Tests.Runner.WebApp.Features.AppLogging.Services;
using Zat.Tests.Runner.WebApp.Shared.Logging;

[TestFixture]
public class DiagnosticsLoggerSinkTests
{
    private List<CapturedRecord> records;
    private Mock<ILoggerFactory> loggerFactoryMock;
    private DiagnosticsLoggerSink unit;

    [SetUp]
    public void SetUp()
    {
        this.records = [];
        this.loggerFactoryMock = new Mock<ILoggerFactory>();
        this.loggerFactoryMock
            .Setup(factory => factory.CreateLogger(It.IsAny<string>()))
            .Returns<string>(category => new CapturingLogger(category, this.records));

        this.unit = new DiagnosticsLoggerSink(this.loggerFactoryMock.Object);
    }

    [TestCase(LogSeverity.Info, ExpectedResult = LogLevel.Information)]
    [TestCase(LogSeverity.Warning, ExpectedResult = LogLevel.Warning)]
    [TestCase(LogSeverity.Error, ExpectedResult = LogLevel.Error)]
    public LogLevel Write__WhenTheEntryHasASeverity__ThenShouldLogItAtTheMatchingLevel(
        LogSeverity givenSeverity)
    {
        // When:
        this.unit.Write(CreateEntry(givenSeverity, LogSources.App));

        // Then:
        return this.records.Single().Level;
    }

    [TestCase(LogSources.App)]
    [TestCase(LogSources.TestRun)]
    [TestCase(LogSources.TestLink)]
    [TestCase("SomeFutureSource")]
    public void Write__WhenTheEntryHasASource__ThenShouldLogItUnderTheCategoryOfThatSource(
        string givenSource)
    {
        // When:
        this.unit.Write(CreateEntry(LogSeverity.Info, givenSource));

        // Then:
        // The category is what the standard Logging:<provider>:LogLevel configuration filters on,
        // so one channel has to be switchable without touching the others.
        Assert.That(this.records.Single().Category, Is.EqualTo("Zat.Tests.Runner.TuiAppLog." + givenSource));
    }

    [Test]
    public void GetCategory__WhenAskedForASource__ThenShouldPrefixItSoThatTheApplicationFiltersMatch()
    {
        // When:
        var result = DiagnosticsLoggerSink.GetCategory(LogSources.TestRun);

        // Then:
        Assert.That(result, Is.EqualTo(DiagnosticsLoggerSink.CategoryPrefix + LogSources.TestRun));
    }

    [Test]
    public void Write__WhenTheEntryHasNoDetail__ThenShouldLogTheMessageAlone()
    {
        // Given:
        const string givenMessage = "Test run finished.";

        // When:
        this.unit.Write(new LogEntry(DateTimeOffset.Now, LogSeverity.Info, LogSources.App, givenMessage));

        // Then:
        Assert.That(this.records.Single().Message, Is.EqualTo(givenMessage));
    }

    [Test]
    public void Write__WhenTheEntryHasDetail__ThenShouldLogItAsTheLinesFollowingTheMessage()
    {
        // Given:
        const string givenMessage = "Test run failed.";
        const string givenDetail = "line 1\nline 2";

        // When:
        this.unit.Write(
            new LogEntry(DateTimeOffset.Now, LogSeverity.Error, LogSources.TestRun, givenMessage, givenDetail));

        // Then:
        Assert.That(this.records.Single().Message, Is.EqualTo(givenMessage + "\n" + givenDetail));
    }

    [Test]
    public void Write__WhenSeveralEntriesShareASource__ThenShouldCreateTheCategoryLoggerOnlyOnce()
    {
        // When:
        this.unit.Write(CreateEntry(LogSeverity.Info, LogSources.App));
        this.unit.Write(CreateEntry(LogSeverity.Error, LogSources.App));

        // Then:
        this.loggerFactoryMock.Verify(factory => factory.CreateLogger(It.IsAny<string>()), Times.Once);
    }

    [Test]
    public void Write__WhenEntriesComeFromDifferentSources__ThenShouldCreateOneLoggerPerSource()
    {
        // When:
        this.unit.Write(CreateEntry(LogSeverity.Info, LogSources.App));
        this.unit.Write(CreateEntry(LogSeverity.Info, LogSources.TestRun));

        // Then:
        this.loggerFactoryMock.Verify(factory => factory.CreateLogger(It.IsAny<string>()), Times.Exactly(2));
    }

    private static LogEntry CreateEntry(LogSeverity severity, string source)
    {
        return new LogEntry(DateTimeOffset.Now, severity, source, "message");
    }

    private sealed record CapturedRecord(string Category, LogLevel Level, string Message);

    private sealed class CapturingLogger(string category, List<CapturedRecord> records) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            records.Add(new CapturedRecord(category, logLevel, formatter(state, exception)));
        }
    }
}