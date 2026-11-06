namespace Zat.Tests.Runner.WebApp.Tests.Components.Primitives;

using AngleSharp.Dom;
using Bunit;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentValidation;

using Fluxor;

using Microsoft.Extensions.DependencyInjection;

using Moq;

using NUnit.Framework;

using Zat.Tests.Runner.WebApp.Components.Primitives;
using Zat.Tests.Runner.WebApp.Shared.Validation;
using Zat.Tests.Runner.WebApp.Shared.ViewModel;

/// <summary>
/// What the input shows, what it writes back and when it writes it.
/// </summary>
/// <remarks>
/// What a binding may name and what it asks the view model about is exercised here too, because
/// <see cref="BindingComponentBase{TViewModel, TBindingValue}"/> has no markup of its own and
/// cannot be drawn without a control to draw it.
/// </remarks>
[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public partial class BindingInputTests : Bunit.TestContext
{
    private const string MessageSelector = ".validity-message";

    private const string GivenVersion = "3.1.4";
    private const string GivenMessage = "Not a version.";

    private const string GivenLetters = "chosen";
    private const string GivenNonsense = "1234";
    private const string ValidationMessage = "Letters only.";

    private readonly EditedViewModel viewModel = new();

    [SetUp]
    public void SetUp()

        // A binding control is a Fluxor component by inheritance, which is all this is here for.
        => this.Services.AddSingleton(new Mock<IActionSubscriber>().Object);

    [TearDown]
    public void TearDown()
        => this.Dispose();

    [Test]
    public void Render__WhenViewModelHoldsValue__ThenShouldShowIt()
    {
        // Given:
        this.viewModel.Text = GivenVersion;

        // When:
        var component = this.RenderTextInput();

        // Then:
        Assert.That(component.Find(TagNames.Input).GetAttribute(AttributeNames.Value), Is.EqualTo(GivenVersion));
    }

    [Test]
    public void Edit__WhenTesterHasFinishedTyping__ThenShouldWriteItToViewModel()
    {
        // Given:
        var component = this.RenderTextInput();

        // When:
        component.Find(TagNames.Input).Change(GivenVersion);

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(this.viewModel.Text, Is.EqualTo(GivenVersion));
            Assert.That(component.Find(TagNames.Input).GetAttribute(AttributeNames.Value), Is.EqualTo(GivenVersion));
        }
    }

    [Test]
    public void Edit__WhenEditHasBeenWritten__ThenShouldTellWhoeverAskedToHearOfIt()
    {
        // Given:
        string? told = null;

        var component = this.RenderTextInput(
            parameters => parameters.Add(input => input.OnChange, value => told = value));

        // When:
        component.Find(TagNames.Input).Change(GivenVersion);

        // Then:
        Assert.That(told, Is.EqualTo(GivenVersion));
    }

    [Test]
    public void Edit__WhenWhatWasTypedCannotBeHeld__ThenShouldGoBackToValueThatIs()
    {
        // Given:
        // The view model never sees the text, so the input is the only place it could be left — and
        // leaving it there would show the tester a value the view model does not hold.
        this.viewModel.Number = 7;

        var component =
            this.RenderComponent<BindingInput<EditedViewModel, int>>(
                parameters => parameters
                    .AddCascadingValue(this.viewModel)
                    .Add(input => input.Binding, model => model.Number));

        // When:
        component.Find(TagNames.Input).Change("not a number");

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(this.viewModel.Number, Is.EqualTo(7));
            Assert.That(component.Find(TagNames.Input).GetAttribute(AttributeNames.Value), Is.EqualTo("7"));
        }
    }

    [Test]
    public void Edit__WhenBindingEventIsOnInput__ThenShouldWriteWhileTesterIsStillTyping()
    {
        // Given:
        var component = this.RenderTextInput(
            parameters => parameters.Add(input => input.BindingEvent, BindingEvent.OnInput));

        // When:
        component.Find(TagNames.Input).Input(GivenVersion);

        // Then:
        Assert.That(this.viewModel.Text, Is.EqualTo(GivenVersion));
    }

    [Test]
    public void Edit__WhenBindingEventIsLeftUnset__ThenShouldNotListenWhileTesterIsStillTyping()
    {
        // Given:
        var component = this.RenderTextInput();

        // When / Then:
        // Nothing is listening for the keystroke at all, which is what an edit heard only once it
        // is finished comes down to.
        Assert.That(
            () => component.Find(TagNames.Input).Input(GivenVersion),
            Throws.InstanceOf<MissingEventHandlerException>());
    }

    [Test]
    public void Render__WhenViewModelSaysWhatIsWrongWithValue__ThenShouldShowIt()
    {
        // Given:
        this.viewModel.SaySomethingIsWrongWith(
            nameof(EditedViewModel.Text),
            new Validity([new Validity.Issue(nameof(EditedViewModel.Text), GivenMessage)]));

        // When:
        var component = this.RenderTextInput();

        // Then:
        Assert.That(component.Find(MessageSelector).TextContent.Trim(), Is.EqualTo(GivenMessage));
    }

    [Test]
    public void Binding__WhenItNamesAnythingOtherThanPropertyOfViewModel__ThenShouldThrow()

        // A control writes the edit back to the view model it was cascaded, so a binding reaching
        // through a property is a mistake in the markup rather than something to find out about at
        // the first edit.
        => Assert.That(
            () => this.RenderComponent<BindingInput<EditedViewModel, int>>(
                parameters => parameters
                    .AddCascadingValue(this.viewModel)
                    .Add(input => input.Binding, model => model.Text!.Length)),
            Throws.InstanceOf<InvalidOperationException>());

    [Test]
    public void Binding__WhenItNamesPropertyThatCannotBeWrittenTo__ThenShouldThrow()
        => Assert.That(
            () => this.RenderComponent<BindingInput<EditedViewModel, string>>(
                parameters => parameters
                    .AddCascadingValue(this.viewModel)
                    .Add(input => input.Binding, model => model.Fixed)),
            Throws.InstanceOf<InvalidOperationException>());

    [Test]
    public void Render__WhenControlStandsOutsideDataContext__ThenShouldThrow()
        => Assert.That(
            () => this.RenderComponent<BindingInput<EditedViewModel, string?>>(
                parameters => parameters.Add(input => input.Binding, model => model.Text)),
            Throws.InstanceOf<InvalidOperationException>());

    [Test]
    public void Edit__WhenEditIsWrong__ThenShouldHandItToViewModelAndShowWhatIsWrong()
    {
        // Given:
        // The view model holds a wrong value all the same, so the input keeps showing what was
        // typed and says what is wrong with it.
        ValidatingViewModel validating = new() { Name = GivenLetters };

        var component =
            this.RenderComponent<BindingInput<ValidatingViewModel, string>>(
                parameters => parameters
                    .AddCascadingValue(validating)
                    .Add(input => input.Binding, model => model.Name));

        // When:
        component.Find(TagNames.Input).Change(GivenNonsense);

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(validating.Name, Is.EqualTo(GivenNonsense));
            Assert.That(component.Find(TagNames.Input).GetAttribute(AttributeNames.Value), Is.EqualTo(GivenNonsense));
            Assert.That(component.Find(MessageSelector).TextContent.Trim(), Is.EqualTo(ValidationMessage));
        }
    }

    private IRenderedComponent<BindingInput<EditedViewModel, string?>> RenderTextInput(
        Action<ComponentParameterCollectionBuilder<BindingInput<EditedViewModel, string?>>>? added = null)
        => this.RenderComponent<BindingInput<EditedViewModel, string?>>(
            parameters =>
            {
                parameters
                    .AddCascadingValue(this.viewModel)
                    .Add(input => input.Binding, model => model.Text);

                added?.Invoke(parameters);
            });

    /// <summary>
    /// A real view model, which says what is wrong with an edit.
    /// </summary>
    private sealed partial class ValidatingViewModel : ViewModelBase
    {
        public ValidatingViewModel()
            => this.InitValidator(
                this,
                validator => validator
                    .RuleFor(model => model.Name)
                    .Matches("^[a-z]+$")
                    .WithMessage(ValidationMessage));

        // ReSharper disable once PropertyCanBeMadeInitOnly.Local
        [ObservableProperty]
        public partial string Name { get; set; } = string.Empty;
    }
}