namespace Zat.Tests.Runner.WebApp.Tests.Shared.JsInterop;

using Bunit;

using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

using Moq;

using NUnit.Framework;

using Zat.Tests.Runner.WebApp.Shared.JsInterop;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class JsModuleInteropTests
{
    private const string ModulePath = "./Components/Layout/SplitterBar.razor.js";
    private const string ImportIdentifier = "import";
    private const string GivenFunction = "initialize";

    private BunitJSInterop jsInterop;
    private Mock<ILogger<JsModuleInterop>> loggerMock;

    [SetUp]
    public void SetUp()
    {
        this.jsInterop = new BunitJSInterop();
        this.loggerMock = new Mock<ILogger<JsModuleInterop>>();
    }

    [Test]
    public async Task InvokeVoidSafeAsync__WhenTheFunctionSucceeds__ThenShouldInvokeItOnTheModule()
    {
        // Given:
        var givenModule = this.jsInterop.SetupModule(ModulePath);
        var givenFunction = givenModule.SetupVoid(GivenFunction, _ => true).SetVoidResult();
        var unit = this.CreateUnit();

        // When:
        await unit.InvokeVoidSafeAsync(GivenFunction, "argument");

        // Then:
        Assert.That(givenFunction.Invocations[GivenFunction], Has.Count.EqualTo(1));
    }

    [Test]
    public void InvokeVoidSafeAsync__WhenTheFunctionFails__ThenShouldNotThrow()
    {
        // Given:
        var unit = this.CreateUnitWithFailingFunction(new InvalidOperationException("boom"));

        // When / Then:
        Assert.That(async () => await unit.InvokeVoidSafeAsync(GivenFunction), Throws.Nothing);
    }

    [Test]
    public async Task InvokeVoidSafeAsync__WhenTheFunctionFails__ThenShouldReportItToTheLoggingPipeline()
    {
        // Given:
        var unit = this.CreateUnitWithFailingFunction(new InvalidOperationException("boom"));

        // When:
        await unit.InvokeVoidSafeAsync(GivenFunction);

        // Then:
        this.VerifyErrorLogged(Times.Once());
    }

    [Test]
    public async Task InvokeVoidSafeAsync__WhenTheModuleCannotBeImported__ThenShouldReportItAndNotThrow()
    {
        // Given:
        // The module is never set up, so importing it fails the way a missing .razor.js would.
        var unit = this.CreateUnit();

        // When / Then:
        Assert.That(async () => await unit.InvokeVoidSafeAsync(GivenFunction), Throws.Nothing);

        await unit.InvokeVoidSafeAsync(GivenFunction);
        this.VerifyErrorLogged(Times.Exactly(2));
    }

    [Test]
    public async Task InvokeVoidSafeAsync__WhenTheImportFailedBefore__ThenShouldNotImportAgain()
    {
        // Given:
        var unit = this.CreateUnit();
        await unit.InvokeVoidSafeAsync(GivenFunction);

        // When:
        await unit.InvokeVoidSafeAsync(GivenFunction);

        // Then:
        Assert.That(this.jsInterop.Invocations[ImportIdentifier], Has.Count.EqualTo(1));
    }

    [Test]
    public async Task InvokeVoidSafeAsync__WhenTheBrowserIsAlreadyGone__ThenShouldStaySilent(
        [ValueSource(nameof(BrowserGoneExceptions))]
        Exception givenException)
    {
        // Given:
        var unit = this.CreateUnitWithFailingFunction(givenException);

        // When:
        await unit.InvokeVoidSafeAsync(GivenFunction);

        // Then:
        this.VerifyErrorLogged(Times.Never());
    }

    [Test]
    public void InvokeVoidSafeAsync__WhenTheBrowserIsAlreadyGone__ThenShouldNotThrow(
        [ValueSource(nameof(BrowserGoneExceptions))]
        Exception givenException)
    {
        // Given:
        var unit = this.CreateUnitWithFailingFunction(givenException);

        // When / Then:
        Assert.That(async () => await unit.InvokeVoidSafeAsync(GivenFunction), Throws.Nothing);
    }

    [Test]
    public void DisposeAsync__WhenNoCallWasEverMade__ThenShouldNotTouchTheBrowser()
    {
        // Given:
        var unit = this.CreateUnit();

        // When:
        Assert.That(async () => await unit.DisposeAsync(), Throws.Nothing);

        // Then:
        Assert.That(this.jsInterop.Invocations, Is.Empty);
    }

    [Test]
    public async Task DisposeAsync__WhenTheImportFailed__ThenShouldNotThrowAndNotReportAgain()
    {
        // Given:
        var unit = this.CreateUnit();
        await unit.InvokeVoidSafeAsync(GivenFunction);
        this.loggerMock.Reset();

        // When / Then:
        Assert.That(async () => await unit.DisposeAsync(), Throws.Nothing);
        this.VerifyErrorLogged(Times.Never());
    }

    private static IEnumerable<Exception> BrowserGoneExceptions()
    {
        yield return new JSDisconnectedException("The circuit disconnected.");
        yield return new TaskCanceledException("The circuit is shutting down.");
    }

    private JsModuleInterop CreateUnit()
        => new(this.jsInterop.JSRuntime, ModulePath, this.loggerMock.Object);

    private JsModuleInterop CreateUnitWithFailingFunction(Exception exception)
    {
        var module = this.jsInterop.SetupModule(ModulePath);
        module.SetupVoid(GivenFunction, _ => true).SetException(exception);

        return this.CreateUnit();
    }

    private void VerifyErrorLogged(Times times)
        => this.loggerMock.Verify(
            logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            times);
}