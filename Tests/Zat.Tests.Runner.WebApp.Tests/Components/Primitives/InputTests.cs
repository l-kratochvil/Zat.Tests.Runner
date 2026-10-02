namespace Zat.Tests.Runner.WebApp.Tests.Components.Primitives;

using Bunit;

using NUnit.Framework;

using Zat.Tests.Runner.WebApp.Components.Primitives;
using Zat.Tests.Runner.WebApp.Shared.Validation;

/// <summary>
/// What the input shows, what it hands on and what it turns down.
/// </summary>
/// <remarks>
/// The value belongs to whoever placed the input, so what is asked here is what the caller is told
/// and when — not what the input holds, because it holds nothing.
/// </remarks>
[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class InputTests : Bunit.TestContext
{
    private const string InputSelector = "input";
    private const string MessageSelector = ".validity-message";

    private const string GivenVersion = "3.1.4";
    private const string GivenMessage = "Not a version.";

    [TearDown]
    public void TearDown()
        => this.Dispose();

    [Test]
    public void Render__WhenValueIsGiven__ThenShouldShowIt()
    {
        // When:
        var component = this.RenderInput(GivenVersion);

        // Then:
        Assert.That(component.Find(InputSelector).GetAttribute("value"), Is.EqualTo(GivenVersion));
    }

    [Test]
    public void Edit__WhenNoValidatorIsGiven__ThenShouldTellCallerOfEdit()
    {
        // Given:
        string? told = null;

        var component = this.RenderInput(
            value: null,
            parameters => parameters.Add(input => input.ValueChanged, edited => told = edited));

        // When:
        component.Find(InputSelector).Change(GivenVersion);

        // Then:
        Assert.That(told, Is.EqualTo(GivenVersion));
    }

    [Test]
    public void Edit__WhenValueIsNotText__ThenShouldConvertItBeforeTellingCaller()
    {
        // Given:
        int? told = null;

        var component = this.RenderComponent<Input<int>>(
            parameters => parameters
                .Add(input => input.Value, 0)
                .Add(input => input.ValueChanged, edited => told = edited));

        // When:
        component.Find(InputSelector).Change("42");

        // Then:
        Assert.That(told, Is.EqualTo(42));
    }

    [Test]
    public void Edit__WhenValidatorFindsError__ThenShouldNotTellCaller()
    {
        // Given:
        var told = false;

        var component = this.RenderInput(
            GivenVersion,
            parameters => parameters
                .Add(input => input.Validator, NothingIsEverRight)
                .Add(input => input.ValueChanged, _ => told = true));

        // When:
        component.Find(InputSelector).Change("nonsense");

        // Then:
        Assert.That(told, Is.False);
    }

    [Test]
    public void Edit__WhenValidatorFindsError__ThenShouldStillSayWhatIsWrongWithIt()
    {
        // Given:
        var component = this.RenderInput(
            GivenVersion,
            parameters => parameters.Add(input => input.Validator, NothingIsEverRight));

        // When:
        component.Find(InputSelector).Change("nonsense");

        // Then:
        // Turning an edit down without a word would leave the tester looking at a value that came
        // back on its own.
        Assert.That(component.Find(MessageSelector).TextContent.Trim(), Is.EqualTo(GivenMessage));
    }

    [Test]
    public void Edit__WhenValidatorFindsError_AndWriteOnErrorIsSet__ThenShouldTellCallerAnyway()
    {
        // Given:
        string? told = null;

        var component = this.RenderInput(
            GivenVersion,
            parameters => parameters
                .Add(input => input.Validator, NothingIsEverRight)
                .Add(input => input.WriteOnError, true)
                .Add(input => input.ValueChanged, edited => told = edited));

        // When:
        component.Find(InputSelector).Change("nonsense");

        // Then:
        Assert.That(told, Is.EqualTo("nonsense"));
    }

    [Test]
    public void Edit__WhenValidatorOnlyWarns__ThenShouldTellCaller()
    {
        // Given:
        string? told = null;

        var component = this.RenderInput(
            value: null,
            parameters => parameters
                .Add(input => input.Validator, SomethingIsWorthKnowing)
                .Add(input => input.ValueChanged, edited => told = edited));

        // When:
        component.Find(InputSelector).Change(GivenVersion);

        // Then:
        // A warning says the value can be used, so holding it back would be the input disagreeing
        // with the validator.
        Assert.That(told, Is.EqualTo(GivenVersion));
    }

    [Test]
    public void Render__WhenValueGivenIsAlreadyWrong__ThenShouldSaySoBeforeAnyEdit()
    {
        // When:
        var component = this.RenderInput(
            GivenVersion,
            parameters => parameters.Add(input => input.Validator, NothingIsEverRight));

        // Then:
        Assert.That(component.Find(MessageSelector).TextContent.Trim(), Is.EqualTo(GivenMessage));
    }

    [Test]
    public void Edit__WhenBindingEventIsOnInput__ThenShouldTellCallerWhileTesterTypes()
    {
        // Given:
        string? told = null;

        var component = this.RenderInput(
            value: null,
            parameters => parameters
                .Add(input => input.BindingEvent, BindingEvent.OnInput)
                .Add(input => input.ValueChanged, edited => told = edited));

        // When:
        component.Find(InputSelector).Input(GivenVersion);

        // Then:
        Assert.That(told, Is.EqualTo(GivenVersion));
    }

    private static Validity NothingIsEverRight(string? value)
        => new([new Validity.Issue(nameof(Input<string>.Value), GivenMessage)]);

    private static Validity SomethingIsWorthKnowing(string? value)
        => new([
            new Validity.Issue(
                nameof(Input<string>.Value), GivenMessage, Validity.Severity.Warning)
        ]);

    private IRenderedComponent<Input<string>> RenderInput(
        string? value,
        Action<ComponentParameterCollectionBuilder<Input<string>>>? added = null)
        => this.RenderComponent<Input<string>>(
            parameters =>
            {
                parameters.Add(input => input.Value, value);

                added?.Invoke(parameters);
            });
}