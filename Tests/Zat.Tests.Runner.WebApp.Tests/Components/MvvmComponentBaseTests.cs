namespace Zat.Tests.Runner.WebApp.Tests.Components;

using Bunit;

using Microsoft.AspNetCore.Components.Rendering;

using NUnit.Framework;

using Zat.Tests.Runner.WebApp.Components;
using Zat.Tests.Runner.WebApp.Tests.Components.Primitives;

/// <summary>
/// That a component redraws on a change of its view model, no more often than it allows.
/// </summary>
[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class MvvmComponentBaseTests : Bunit.TestContext
{
    private readonly EditedViewModel viewModel = new();

    [TearDown]
    public void TearDown()
        => this.Dispose();

    [Test]
    public void RequestRender__WhenNoLimitIsSet__ThenShouldRedrawOnEveryChange()
    {
        // Given:
        var component = this.Render<UnlimitedComponent>();
        this.ChangeViewModel();
        component.WaitForState(() => component.RenderCount == 2);

        // When:
        this.ChangeViewModel();

        // Then:
        component.WaitForState(() => component.RenderCount == 3);
    }

    [Test]
    public void RequestRender__WhenChangeComesSoonerThanLimitAllows__ThenShouldRedrawItLater()
    {
        // Given:
        var component = this.Render<LimitedComponent>();
        this.ChangeViewModel();
        component.WaitForState(() => component.RenderCount == 2);

        // When:
        this.ChangeViewModel();

        // Then:
        Thread.Sleep(TimeSpan.FromMilliseconds(100));
        Assert.That(component.RenderCount, Is.EqualTo(2));
        component.WaitForState(() => component.RenderCount == 3, timeout: TimeSpan.FromSeconds(2));
    }

    [Test]
    public void DisposeAsync__WhenViewModelChangesAfterwards__ThenShouldNotRedraw()
    {
        // Given:
        var component = this.Render<UnlimitedComponent>();

        // When:
        this.DisposeComponents();
        this.ChangeViewModel();

        // Then:
        Thread.Sleep(TimeSpan.FromMilliseconds(100));
        Assert.That(component.RenderCount, Is.EqualTo(1));
    }

    private IRenderedComponent<TComponent> Render<TComponent>()
        where TComponent : MvvmComponentBase<EditedViewModel>
        => this.RenderComponent<TComponent>(parameters => parameters.AddCascadingValue(this.viewModel));

    private void ChangeViewModel()
        => this.viewModel.Number++;

    private class UnlimitedComponent : MvvmComponentBase<EditedViewModel>
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
            => builder.AddContent(0, this.ViewModel.Number);
    }

    private sealed class LimitedComponent : UnlimitedComponent
    {
        protected override byte MaxRendersPerSecond
            => 2;
    }
}