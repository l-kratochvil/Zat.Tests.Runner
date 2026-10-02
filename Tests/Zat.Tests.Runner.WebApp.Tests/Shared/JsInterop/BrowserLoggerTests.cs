namespace Zat.Tests.Runner.WebApp.Tests.Shared.JsInterop;

using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Zat.Tests.Runner.WebApp.Shared.JsInterop;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class BrowserLoggerTests
{
    private const string GivenModule = "Components/Layout/SplitterBar";
    private const string GivenMessage = "The handle lost its pointer capture.";

    private Mock<ILogger> loggerMock;
    private BrowserLogger unit;

    [SetUp]
    public void SetUp()
    {
        this.loggerMock = new Mock<ILogger>();

        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(factory => factory.CreateLogger(It.IsAny<string>())).Returns(this.loggerMock.Object);

        this.unit = new BrowserLogger(loggerFactoryMock.Object);
    }

    [Test]
    public void Constructor__WhenLoggerIsCreated__ThenShouldUseBrowserCategory()
    {
        // Given:
        var givenFactoryMock = new Mock<ILoggerFactory>();
        givenFactoryMock.Setup(factory => factory.CreateLogger(It.IsAny<string>())).Returns(this.loggerMock.Object);

        // When:
        _ = new BrowserLogger(givenFactoryMock.Object);

        // Then:
        givenFactoryMock.Verify(factory => factory.CreateLogger(BrowserLogger.Category), Times.Once());
    }

    [TestCase("debug", LogLevel.Debug)]
    [TestCase("info", LogLevel.Information)]
    [TestCase("warn", LogLevel.Warning)]
    [TestCase("error", LogLevel.Error)]
    [TestCase("ERROR", LogLevel.Error)]
    public void Log__WhenLevelIsKnown__ThenShouldWriteAtMatchingLevel(
        string givenLevel,
        LogLevel expectedLevel)
    {
        // When:
        this.unit.Log(new BrowserDiagnostic(givenLevel, GivenModule, GivenMessage, Detail: null));

        // Then:
        this.VerifyLogged(expectedLevel, Times.Once());
    }

    [TestCase("verbose")]
    [TestCase("")]
    [TestCase(null)]
    public void Log__WhenLevelIsUnknown__ThenShouldKeepRecordAsWarning(string? givenLevel)
    {
        // Given:
        // An unrecognized level is a fault of the calling script; dropping the record would hide
        // both the record and the fault.

        // When:
        this.unit.Log(new BrowserDiagnostic(givenLevel, GivenModule, GivenMessage, Detail: null));

        // Then:
        this.VerifyLogged(LogLevel.Warning, Times.Once());
    }

    [Test]
    public void Log__WhenThereIsNoDetail__ThenShouldWriteModuleAndMessage()
    {
        // When:
        this.unit.Log(new BrowserDiagnostic("info", GivenModule, GivenMessage, Detail: null));

        // Then:
        Assert.That(this.GetWrittenRecord(), Is.EqualTo($"[{GivenModule}] {GivenMessage}"));
    }

    [Test]
    public void Log__WhenDetailIsEmpty__ThenShouldWriteMessageAlone()
    {
        // Given:
        // An empty detail is the same as none: a blank line would only make the file harder to read.

        // When:
        this.unit.Log(new BrowserDiagnostic("info", GivenModule, GivenMessage, Detail: string.Empty));

        // Then:
        Assert.That(this.GetWrittenRecord(), Is.EqualTo($"[{GivenModule}] {GivenMessage}"));
    }

    [Test]
    public void Log__WhenThereIsDetail__ThenShouldWriteItOnFollowingLines()
    {
        // Given:
        const string givenDetail = "at initialize (SplitterBar.razor.js:14:3)";

        // When:
        this.unit.Log(new BrowserDiagnostic("error", GivenModule, GivenMessage, givenDetail));

        // Then:
        Assert.That(this.GetWrittenRecord(), Is.EqualTo($"[{GivenModule}] {GivenMessage}\n{givenDetail}"));
    }

    [Test]
    public void Log__WhenDiagnosticIsMissing__ThenShouldThrow()
    {
        // When / Then:
        Assert.That(() => this.unit.Log(null!), Throws.ArgumentNullException);
    }

    private string GetWrittenRecord()
    {
        var written = this.loggerMock.Invocations.Single(invocation => invocation.Method.Name == nameof(ILogger.Log));

        // The formatter is the last argument of ILogger.Log; invoking it renders the state the way
        // a text destination such as the log file would.
        var state = written.Arguments[2];
        var formatter = (Delegate)written.Arguments[4];

        return (string)formatter.DynamicInvoke(state, null)!;
    }

    private void VerifyLogged(LogLevel level, Times times)
        => this.loggerMock.Verify(
            logger => logger.Log(
                level,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            times);
}