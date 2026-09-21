namespace Zat.Tests.Runner.WebApp.Tests.Components.Layout;

using Bunit;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using NUnit.Framework;

using Zat.Tests.Runner.WebApp.Components.Layout;
using Zat.Tests.Runner.WebApp.Shared.JsInterop;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class SplitterBarTests : Bunit.TestContext
{
    private const string ModulePath = "./Components/Layout/SplitterBar.razor.js";
    private const string InitializeFunction = "initialize";
    private const string DisposeFunction = "dispose";
    private const string HandleSelector = ".splitter-bar";

    private const int HandleArgumentIndex = 0;
    private const int OptionsArgumentIndex = 1;

    private const string GivenCssVariable = "--app-log-height";
    private const string GivenStorageKey = "log-height";
    private const int GivenMinSize = 100;
    private const bool GivenIsHorizontal = true;
    private const double GivenMaxSizeRatio = 0.6;
    private const double GivenDefaultSizeRatio = 0.3;
    private const string GivenLabel = "Different label";

    private BunitJSModuleInterop module;

    [SetUp]
    public void SetUp()
    {
        this.module = this.JSInterop.SetupModule(ModulePath);

        // The component asks the factory for its module, so the real wiring is registered here
        // rather than the component being handed a wrapper built by the test.
        this.Services.AddSingleton<ILogger<JsModuleInterop>>(NullLogger<JsModuleInterop>.Instance);
        this.Services.AddScoped<IJsModuleInteropFactory, JsModuleInteropFactory>();
    }

    [TearDown]
    public void TearDown()
        => this.Dispose();

    [Test]
    public void OnAfterRenderAsync__WhenTheComponentIsFirstRendered__ThenShouldInitializeTheJsModuleWithTheHandleAndOptions()
    {
        // Given:
        var initialize = this.SetupVoidFunction(InitializeFunction);

        // When:
        var component = this.RenderSplitterBar();

        // Then:
        Assert.That(initialize.Invocations[InitializeFunction], Has.Count.EqualTo(1));

        var invocation = initialize.Invocations[InitializeFunction][0];
        invocation.Arguments[HandleArgumentIndex].ShouldBeElementReferenceTo(component.Find(HandleSelector));

        var options = invocation.Arguments[OptionsArgumentIndex]!;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(GetProperty(options, nameof(SplitterBar.CssVariable)), Is.EqualTo(GivenCssVariable));
            Assert.That(GetProperty(options, nameof(SplitterBar.StorageKey)), Is.EqualTo(GivenStorageKey));
            Assert.That(GetProperty(options, nameof(SplitterBar.MinSize)), Is.EqualTo(GivenMinSize));
            Assert.That(GetProperty(options, nameof(SplitterBar.IsHorizontal)), Is.EqualTo(GivenIsHorizontal));
            Assert.That(GetProperty(options, nameof(SplitterBar.MaxSizeRatio)), Is.EqualTo(GivenMaxSizeRatio));
            Assert.That(GetProperty(options, nameof(SplitterBar.DefaultSizeRatio)), Is.EqualTo(GivenDefaultSizeRatio));
        }
    }

    [Test]
    public void OnAfterRenderAsync__WhenTheComponentIsRerendered__ThenShouldInitializeTheJsModuleOnlyOnce()
    {
        // Given:
        var initialize = this.SetupVoidFunction(InitializeFunction);
        var component = this.RenderSplitterBar();

        // When:
        component.SetParametersAndRender(parameters => parameters.Add(p => p.Label, GivenLabel));

        // Then:
        Assert.That(initialize.Invocations[InitializeFunction], Has.Count.EqualTo(1));
    }

    [Test]
    public async Task DisposeAsync__WhenTheComponentWasInitialized__ThenShouldInvokeTheJsDisposeWithTheHandle()
    {
        // Given:
        this.SetupVoidFunction(InitializeFunction);
        var dispose = this.SetupVoidFunction(DisposeFunction);
        var component = this.RenderSplitterBar();

        // When:
        await component.Instance.DisposeAsync();

        // Then:
        Assert.That(dispose.Invocations[DisposeFunction], Has.Count.EqualTo(1));
        dispose.Invocations[DisposeFunction][0].Arguments[HandleArgumentIndex]
            .ShouldBeElementReferenceTo(component.Find(HandleSelector));
    }

    [Test]
    public void DisposeAsync__WhenTheComponentWasNeverRendered__ThenShouldNotThrow()
    {
        // Given:
        SplitterBar unit = new();

        // When / Then:
        Assert.That(async () => await unit.DisposeAsync(), Throws.Nothing);
    }

    private static object? GetProperty(object source, string name)
        => source.GetType().GetProperty(name)!.GetValue(source);

    private JSRuntimeInvocationHandler SetupVoidFunction(string identifier)
        => this.module.SetupVoid(identifier, _ => true).SetVoidResult();

    private IRenderedComponent<SplitterBar> RenderSplitterBar()
        => this.RenderComponent<SplitterBar>(parameters => parameters
            .Add(p => p.CssVariable, GivenCssVariable)
            .Add(p => p.StorageKey, GivenStorageKey)
            .Add(p => p.MinSize, GivenMinSize)
            .Add(p => p.IsHorizontal, GivenIsHorizontal)
            .Add(p => p.MaxSizeRatio, GivenMaxSizeRatio)
            .Add(p => p.DefaultSizeRatio, GivenDefaultSizeRatio));
}