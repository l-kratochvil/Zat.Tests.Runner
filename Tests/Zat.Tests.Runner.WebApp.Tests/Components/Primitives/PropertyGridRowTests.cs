namespace Zat.Tests.Runner.WebApp.Tests.Components.Primitives;

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
    private const string RowSelector = "label.property-grid-row";

    private const string GivenLabel = "IDE version";

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
            Assert.That(component.FindAll($"{RowSelector} input"), Has.Exactly(1).Items);
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
        builder.OpenElement(0, "input");
        builder.CloseElement();
    }

    private IRenderedComponent<PropertyGrid> RenderRowInGrid()
    {
        RenderFragment input = BuildInput;

        return this.RenderComponent<PropertyGrid>(
            parameters => parameters.AddChildContent<PropertyGridRow>(
                row => row
                    .Add(component => component.Label, GivenLabel)
                    .Add(component => component.ChildContent, input)));
    }
}