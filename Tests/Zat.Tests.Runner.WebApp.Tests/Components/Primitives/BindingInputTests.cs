namespace Zat.Tests.Runner.WebApp.Tests.Components.Primitives;

using Bunit;

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
public class BindingInputTests : Bunit.TestContext
{
    private const string InputSelector = "input";
    private const string MessageSelector = ".validity-message";

    private const string GivenVersion = "3.1.4";
    private const string GivenMessage = "Not a version.";

    private const string GivenLetters = "chosen";
    private const string GivenNonsense = "1234";
    private const string RefusalMessage = "Letters only.";

    private readonly EditedViewModel viewModel = new();

    [SetUp]
    public void SetUp()

        // A binding control is a Fluxor component by inheritance, which is all this is here for.
        => this.Services.AddSingleton(new Mock<IActionSubscriber>().Object);

    [TearDown]
    public void TearDown()
        => this.Dispose();

    [Test]
    public void Render__WhenTheViewModelHoldsAValue__ThenShouldShowIt()
    {
        // Given:
        this.viewModel.Text = GivenVersion;

        // When:
        var component = this.RenderTextInput();

        // Then:
        Assert.That(component.Find(InputSelector).GetAttribute("value"), Is.EqualTo(GivenVersion));
    }

    [Test]
    public void Edit__WhenTheTesterHasFinishedTyping__ThenShouldWriteItToTheViewModel()
    {
        // Given:
        var component = this.RenderTextInput();

        // When:
        component.Find(InputSelector).Change(GivenVersion);

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(this.viewModel.Text, Is.EqualTo(GivenVersion));
            Assert.That(component.Find(InputSelector).GetAttribute("value"), Is.EqualTo(GivenVersion));
        }
    }

    [Test]
    public void Edit__WhenTheEditHasBeenWritten__ThenShouldTellWhoeverAskedToHearOfIt()
    {
        // Given:
        string? told = null;

        var component = this.RenderTextInput(
            parameters => parameters.Add(input => input.OnChange, value => told = value));

        // When:
        component.Find(InputSelector).Change(GivenVersion);

        // Then:
        Assert.That(told, Is.EqualTo(GivenVersion));
    }

    [Test]
    public void Edit__WhenWhatWasTypedCannotBeHeld__ThenShouldGoBackToTheValueThatIs()
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
        component.Find(InputSelector).Change("not a number");

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(this.viewModel.Number, Is.EqualTo(7));
            Assert.That(component.Find(InputSelector).GetAttribute("value"), Is.EqualTo("7"));
        }
    }

    [Test]
    public void Edit__WhenTheBindingEventIsOnInput__ThenShouldWriteWhileTheTesterIsStillTyping()
    {
        // Given:
        var component = this.RenderTextInput(
            parameters => parameters.Add(input => input.BindingEvent, BindingEvent.OnInput));

        // When:
        component.Find(InputSelector).Input(GivenVersion);

        // Then:
        Assert.That(this.viewModel.Text, Is.EqualTo(GivenVersion));
    }

    [Test]
    public void Edit__WhenTheBindingEventIsLeftUnset__ThenShouldNotListenWhileTheTesterIsStillTyping()
    {
        // Given:
        var component = this.RenderTextInput();

        // When / Then:
        // Nothing is listening for the keystroke at all, which is what an edit heard only once it
        // is finished comes down to.
        Assert.That(
            () => component.Find(InputSelector).Input(GivenVersion),
            Throws.InstanceOf<MissingEventHandlerException>());
    }

    [Test]
    public void Render__WhenTheViewModelSaysWhatIsWrongWithTheValue__ThenShouldShowIt()
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
    public void Binding__WhenItNamesAnythingOtherThanAPropertyOfTheViewModel__ThenShouldThrow()

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
    public void Binding__WhenItNamesAPropertyThatCannotBeWrittenTo__ThenShouldThrow()
        => Assert.That(
            () => this.RenderComponent<BindingInput<EditedViewModel, string>>(
                parameters => parameters
                    .AddCascadingValue(this.viewModel)
                    .Add(input => input.Binding, model => model.Fixed)),
            Throws.InstanceOf<InvalidOperationException>());

    [Test]
    public void Render__WhenTheControlStandsOutsideADataContext__ThenShouldThrow()
        => Assert.That(
            () => this.RenderComponent<BindingInput<EditedViewModel, string?>>(
                parameters => parameters.Add(input => input.Binding, model => model.Text)),
            Throws.InstanceOf<InvalidOperationException>());

    [Test]
    public void Edit__WhenTheViewModelTurnsTheEditDown__ThenShouldGoBackToShowingWhatIsHeld()
    {
        // Given:
        // A view model that refuses an edit leaves its value where it was, so what sends the input
        // back to showing it is the view model saying the property moved all the same.
        RefusingViewModel refusing = new() { Name = GivenLetters };

        var component =
            this.RenderComponent<BindingInput<RefusingViewModel, string>>(
                parameters => parameters
                    .AddCascadingValue(refusing)
                    .Add(input => input.Binding, model => model.Name));

        // When:
        component.Find(InputSelector).Change(GivenNonsense);

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(refusing.Name, Is.EqualTo(GivenLetters));
            Assert.That(component.Find(InputSelector).GetAttribute("value"), Is.EqualTo(GivenLetters));
            Assert.That(component.Find(MessageSelector).TextContent.Trim(), Is.EqualTo(RefusalMessage));
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
    /// A real view model, which turns down an edit it finds something wrong with.
    /// </summary>
    private sealed class RefusingViewModel : ViewModelBase
    {
        private string name = string.Empty;

        public RefusingViewModel()
            => this.InitValidator(
                this,
                validator => validator
                    .RuleFor(model => model.Name)
                    .Matches("^[a-z]+$")
                    .WithMessage(RefusalMessage));

        public string Name
        {
            get => this.name;
            set => this.SetProperty(ref this.name, value);
        }
    }
}