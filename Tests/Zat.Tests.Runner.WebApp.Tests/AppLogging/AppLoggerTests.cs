namespace Zat.Tests.Runner.WebApp.Tests.AppLogging;

using Moq;

using NUnit.Framework;

using Zat.Tests.Runner.WebApp.Features.AppLogging.Models;
using Zat.Tests.Runner.WebApp.Features.AppLogging.Services;
using Zat.Tests.Runner.WebApp.Shared.Logging;

[TestFixture]
public class AppLoggerTests
{
    private const string GivenSource = LogSources.TestRun;

    private Mock<IAppLoggerHub> loggerHubMock;
    private List<LogEntry> appendedEntries;
    private AppLogger unit;

    [SetUp]
    public void SetUp()
    {
        this.appendedEntries = [];
        this.loggerHubMock = new Mock<IAppLoggerHub>();
        this.loggerHubMock
            .Setup(loggerHub => loggerHub.Append(It.IsAny<LogEntry>()))
            .Callback<LogEntry>(this.appendedEntries.Add);

        this.unit = new AppLogger(this.loggerHubMock.Object, GivenSource);
    }

    [TestCaseSource(nameof(SeverityMethodCases))]
    public LogSeverity Log__WhenCalledThroughASeverityMethod__ThenShouldAppendAnEntryOfThatSeverity(
        Action<IAppLogger> givenCall)
    {
        // When:
        givenCall(this.unit);

        // Then:
        return this.appendedEntries.Single().Severity;
    }

    [TestCase(LogSeverity.Info)]
    [TestCase(LogSeverity.Warning)]
    [TestCase(LogSeverity.Error)]
    public void Log__WhenCalledWithASeverity__ThenShouldAppendAnEntryOfTheLoggersSource(
        LogSeverity givenSeverity)
    {
        // Given:
        const string givenMessage = "message";

        // When:
        this.unit.Log(givenSeverity, givenMessage);

        // Then:
        var entry = this.appendedEntries.Single();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(entry.Severity, Is.EqualTo(givenSeverity));
            Assert.That(entry.Source, Is.EqualTo(GivenSource));
            Assert.That(entry.Message, Is.EqualTo(givenMessage));
        }
    }

    [Test]
    public void Info__WhenCalledWithDetail__ThenShouldAppendAnEntryCarryingThatDetail()
    {
        // Given:
        const string givenMessage = "Test run finished.";
        const string givenDetail = "line 1\nline 2";

        // When:
        this.unit.Info(givenMessage, givenDetail);

        // Then:
        Assert.That(this.appendedEntries.Single().Detail, Is.EqualTo(givenDetail));
    }

    [Test]
    public void Info__WhenCalledWithoutDetail__ThenShouldAppendAnEntryWithoutDetail()
    {
        // When:
        this.unit.Info("Test run finished.");

        // Then:
        Assert.That(this.appendedEntries.Single().Detail, Is.Null);
    }

    [Test]
    public void Source__WhenTheLoggerWasCreated__ThenShouldReturnTheSourceItIsBoundTo()
    {
        // When:
        var result = this.unit.Source;

        // Then:
        Assert.That(result, Is.EqualTo(GivenSource));
    }

    private static IEnumerable<TestCaseData> SeverityMethodCases()
    {
        const string Prefix = nameof(AppLogger.Log);
        const string Message = "message";

        yield return new TestCaseData((Action<IAppLogger>)(logger => logger.Info(Message)))
            .SetName(Prefix + "_WhenCalledThroughInfo_ThenShouldAppend_Info")
            .Returns(LogSeverity.Info);

        yield return new TestCaseData((Action<IAppLogger>)(logger => logger.Warning(Message)))
            .SetName(Prefix + "_WhenCalledThroughWarning_ThenShouldAppend_Warning")
            .Returns(LogSeverity.Warning);

        yield return new TestCaseData((Action<IAppLogger>)(logger => logger.Error(Message)))
            .SetName(Prefix + "_WhenCalledThroughError_ThenShouldAppend_Error")
            .Returns(LogSeverity.Error);
    }
}