namespace Zat.Tests.Runner.WebApp.Tests.Components.Primitives;

using Bunit;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

using NUnit.Framework;

using Zat.Tests.Runner.WebApp.Components.Primitives;
using Zat.Tests.Runner.WebApp.Shared.Validation;

/// <summary>
/// What a row says about one property, and where it may be said at all.
/// </summary>
[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class PropertyGridRowTests : Bunit.TestContext
{
    private const string RowSelector = "label.property-grid-row";
    private const string MessageSelector = ".property-grid-row-message";
    private const string ErrorSelector = ".property-grid-row-message.is-error";
    private const string WarningSelector = ".property-grid-row-message.is-warning";

    private const string GivenLabel = "IDE version";
    private const string GivenFieldName = "IdeVersion";
    private const string GivenMessage = "Not a version.";

    [TearDown]
    public void TearDown()
        => this.Dispose();

    [Test]
    public void Render__WhenTheRowIsInAGrid__ThenShouldWrapTheLabelAndTheInputInOneLabelElement()
    {
        // Given:
        // The input sitting inside the label is what binds the two without anybody writing an
        // identifier for them to agree on.

        // When:
        var component = this.RenderRowInGrid(Validity.Valid);

        // Then:
        Assert.Multiple(() =>
        {
            Assert.That(
                component.Find($"{RowSelector} .property-grid-row-label").TextContent.Trim(),
                Is.EqualTo(GivenLabel));
            Assert.That(component.FindAll($"{RowSelector} input"), Has.Exactly(1).Items);
        });
    }

    [Test]
    public void Render__WhenTheRowIsOutsideAGrid__ThenShouldThrowRatherThanRender()

        // A row outside a grid has no columns to fall into, which would otherwise show up as a
        // quietly broken layout rather than as a mistake.
        => Assert.That(
            () => this.RenderComponent<PropertyGridRow>(
                parameters => parameters.Add(row => row.Label, GivenLabel)),
            Throws.InstanceOf<InvalidOperationException>());

    [Test]
    public void Render__WhenTheValidityHoldsAnError__ThenShouldShowItAsAnError()
    {
        // Given:
        var validity = new Validity([new Validity.Issue(GivenFieldName, GivenMessage)]);

        // When:
        var component = this.RenderRowInGrid(validity);

        // Then:
        Assert.Multiple(() =>
        {
            Assert.That(component.Find(ErrorSelector).TextContent.Trim(), Is.EqualTo(GivenMessage));
            Assert.That(component.FindAll(WarningSelector), Is.Empty);
        });
    }

    [Test]
    public void Render__WhenTheValidityHoldsAWarning__ThenShouldShowItDifferently()
    {
        // Given:
        var validity = new Validity(
            [new Validity.Issue(GivenFieldName, GivenMessage, Validity.Severity.Warning)]);

        // When:
        var component = this.RenderRowInGrid(validity);

        // Then:
        Assert.Multiple(() =>
        {
            Assert.That(component.Find(WarningSelector).TextContent.Trim(), Is.EqualTo(GivenMessage));
            Assert.That(component.FindAll(ErrorSelector), Is.Empty);
        });
    }

    [Test]
    public void Render__WhenNothingIsWrong__ThenShouldShowNoMessage()
    {
        // When:
        var component = this.RenderRowInGrid(Validity.Valid);

        // Then:
        Assert.That(component.FindAll(MessageSelector), Is.Empty);
    }

    private static void BuildInput(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "input");
        builder.CloseElement();
    }

    private IRenderedComponent<PropertyGrid> RenderRowInGrid(Validity validity)
    {
        RenderFragment input = BuildInput;

        return this.RenderComponent<PropertyGrid>(
            parameters => parameters.AddChildContent<PropertyGridRow>(
                row => row
                    .Add(component => component.Label, GivenLabel)
                    .Add(component => component.Validity, validity)
                    .Add(component => component.ChildContent, input)));
    }
}