namespace Zat.Tests.Runner.WebApp.Tests.Components.Primitives;

using AngleSharp.Dom;
using Bunit;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

using NUnit.Framework;

using Zat.Tests.Runner.WebApp.Components.Primitives;

/// <summary>
/// How a row lays out the label and the input of one property, and where it may do so at all.
/// </summary>
/// <remarks>
/// What is wrong with the value is shown by the <c>Validated</c> wrapper of the input, not by the row.
/// </remarks>
[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class PropertyGridRowTests : Bunit.TestContext
{
    private const string GivenLabel = "given-label";

    private static readonly string RowSelector = Html.MakeLabelSelector(".property-grid-row");

    [TearDown]
    public void TearDown()
        => this.Dispose();

    [Test]
    public void Render__WhenRowIsInGrid__ThenShouldWrapLabelAndInputInOneLabelElement()
    {
        // Given:
        // The input sitting inside the label is what binds the two without anybody writing an
        // identifier for them to agree on.

        // When:
        var component = this.RenderRowInGrid();

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(
                component.Find($"{RowSelector} .property-grid-row-label").TextContent.Trim(),
                Is.EqualTo(GivenLabel));
            Assert.That(component.FindAll($"{RowSelector} {TagNames.Input}"), Has.Exactly(1).Items);
        }
    }

    [Test]
    public void Render__WhenRowHoldsGroupOfControls__ThenShouldNotWrapThemInLabelElement()
    {
        // Given:
        // A label belongs to its first control only, so a click on the name would press that control.

        // When:
        var component = this.RenderRowInGrid(useLabel: false);

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(component.FindAll(TagNames.Label), Is.Empty);
            Assert.That(
                component.Find(".property-grid-row .property-grid-row-label").TextContent.Trim(),
                Is.EqualTo(GivenLabel));
        }
    }

    [Test]
    public void Render__WhenRowIsOutsideGrid__ThenShouldThrowRatherThanRender()

        // A row outside a grid has no columns to fall into, which would otherwise show up as a
        // quietly broken layout rather than as a mistake.
        => Assert.That(
            () => this.RenderComponent<PropertyGridRow>(
                parameters => parameters.Add(row => row.Label, GivenLabel)),
            Throws.InstanceOf<InvalidOperationException>());

    private static void BuildInput(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, TagNames.Input);
        builder.CloseElement();
    }

    private IRenderedComponent<PropertyGrid> RenderRowInGrid(bool useLabel = true)
    {
        RenderFragment input = BuildInput;

        return this.RenderComponent<PropertyGrid>(
            parameters => parameters.AddChildContent<PropertyGridRow>(
                row => row
                    .Add(component => component.Label, GivenLabel)
                    .Add(component => component.UseLabel, useLabel)
                    .Add(component => component.ChildContent, input)));
    }
}