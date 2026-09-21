namespace Zat.Tests.Runner.WebApp.Tests.Shared.JsInterop;

using Bunit;

using Microsoft.Extensions.Logging.Abstractions;

using NUnit.Framework;

using Zat.Tests.Runner.WebApp.Shared.JsInterop;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class JsModuleInteropFactoryTests
{
    private const string ModulePath = "./Components/Layout/SplitterBar.razor.js";
    private const string GivenFunction = "initialize";

    private BunitJSInterop jsInterop;
    private JsModuleInteropFactory unit;

    [SetUp]
    public void SetUp()
    {
        this.jsInterop = new BunitJSInterop();
        this.unit = new JsModuleInteropFactory(
            this.jsInterop.JSRuntime,
            NullLogger<JsModuleInterop>.Instance);
    }

    [Test]
    public async Task Create__WhenTheCreatedWrapperIsCalled__ThenShouldReachTheModuleAtTheGivenPath()
    {
        // Given:
        var givenModule = this.jsInterop.SetupModule(ModulePath);
        var givenFunction = givenModule.SetupVoid(GivenFunction, _ => true).SetVoidResult();

        // When:
        await this.unit.Create(ModulePath).InvokeVoidSafeAsync(GivenFunction);

        // Then:
        Assert.That(givenFunction.Invocations[GivenFunction], Has.Count.EqualTo(1));
    }

    [Test]
    public void Create__WhenCalledTwice__ThenShouldReturnSeparateWrappers()
    {
        // Given:
        // Each caller disposes its own wrapper, so sharing one would let the first component
        // released take the module away from the others.

        // When:
        var result = this.unit.Create(ModulePath);

        // Then:
        Assert.That(this.unit.Create(ModulePath), Is.Not.SameAs(result));
    }
}