namespace Zat.Tests.Runner.WebApp.Tests.Components.Primitives;

using Bunit;

using Fluxor;

using Microsoft.Extensions.DependencyInjection;

using Moq;

using NUnit.Framework;

using Zat.Tests.Runner.WebApp.Components.Primitives;

using Shade = Zat.Tests.Runner.WebApp.Tests.Components.Primitives.EditedViewModel.Shade;

/// <summary>
/// What is offered, what marks a choice as made and what a choice writes back.
/// </summary>
[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class BindingSelectTests : Bunit.TestContext
{
    private const string SelectSelector = "select";
    private const string OptionSelector = "option";

    private const string GivenPlaceholder = "—";

    private static readonly Shade?[] GivenOffer = [Shade.Red, Shade.Green];

    private readonly EditedViewModel viewModel = new();

    [SetUp]
    public void SetUp()

        // A binding control is a Fluxor component by inheritance, which is all this is here for.
        => this.Services.AddSingleton(new Mock<IActionSubscriber>().Object);

    [TearDown]
    public void TearDown()
        => this.Dispose();

    [Test]
    public void Render__WhenTheViewModelHoldsAnOfferedValue__ThenShouldPointTheBrowserAtItsPlace()
    {
        // Given:
        this.viewModel.Shading = Shade.Green;

        // When:
        var component = this.RenderSelect();

        // Then:
        // The place among the offered values is the one name every value has, so it is what the
        // browser is pointed at rather than the value spelled out as text.
        Assert.That(component.Find(SelectSelector).GetAttribute("value"), Is.EqualTo("1"));
    }

    [Test]
    public void Render__WhenTheViewModelHoldsNothingThatIsOffered__ThenShouldChooseNothing()
    {
        // Given:
        this.viewModel.Shading = null;

        // When:
        var component = this.RenderSelect();

        // Then:
        Assert.That(component.FindAll(OptionSelector)[0].HasAttribute("selected"), Is.True);
    }

    [Test]
    public void Choose__WhenAnOfferedValueIsChosen__ThenShouldWriteTheValueAndNotItsPlace()
    {
        // Given:
        var component = this.RenderSelect();

        // When:
        component.Find(SelectSelector).Change("1");

        // Then:
        Assert.That(this.viewModel.Shading, Is.EqualTo(Shade.Green));
    }

    [Test]
    public void Choose__WhenThePlaceholderIsChosen__ThenShouldWriteNothing()
    {
        // Given:
        this.viewModel.Shading = Shade.Green;

        var component = this.RenderSelect();

        // When:
        component.Find(SelectSelector).Change(string.Empty);

        // Then:
        Assert.That(this.viewModel.Shading, Is.Null);
    }

    [Test]
    public void Render__WhenNoOptionLabelIsGiven__ThenShouldLetTheValueSpeakForItself()
    {
        // When:
        var component = this.RenderSelect();

        // Then:
        Assert.That(
            component.FindAll(OptionSelector).Select(option => option.TextContent.Trim()),
            Is.EqualTo(new[] { GivenPlaceholder, nameof(Shade.Red), nameof(Shade.Green) }));
    }

    [Test]
    public void Render__WhenAnOptionLabelIsGiven__ThenShouldNameTheOfferedValuesWithIt()
    {
        // When:
        var component = this.RenderSelect(
            parameters => parameters.Add(
                select => select.OptionLabel, shade => $"shade of {shade}"));

        // Then:
        Assert.That(
            component.FindAll(OptionSelector).Select(option => option.TextContent.Trim()),
            Is.EqualTo(new[] { GivenPlaceholder, "shade of Red", "shade of Green" }));
    }

    [Test]
    public void Render__WhenAPlaceholderIsOfferedForAValueTheViewModelMustHold__ThenShouldThrow()

        // A value type holds a value even where the tester meant none, so there is nothing for the
        // placeholder to write back.
        => Assert.That(
            () => this.RenderComponent<BindingSelect<EditedViewModel, Shade>>(
                parameters => parameters
                    .AddCascadingValue(this.viewModel)
                    .Add(select => select.Binding, model => model.RequiredShading)
                    .Add(select => select.Options, [Shade.Red, Shade.Green])
                    .Add(select => select.Placeholder, GivenPlaceholder)),
            Throws.InstanceOf<InvalidOperationException>());

    private IRenderedComponent<BindingSelect<EditedViewModel, Shade?>> RenderSelect(
        Action<ComponentParameterCollectionBuilder<BindingSelect<EditedViewModel, Shade?>>>? added = null)
        => this.RenderComponent<BindingSelect<EditedViewModel, Shade?>>(
            parameters =>
            {
                parameters
                    .AddCascadingValue(this.viewModel)
                    .Add(select => select.Binding, model => model.Shading)
                    .Add(select => select.Options, GivenOffer)
                    .Add(select => select.Placeholder, GivenPlaceholder);

                added?.Invoke(parameters);
            });
}