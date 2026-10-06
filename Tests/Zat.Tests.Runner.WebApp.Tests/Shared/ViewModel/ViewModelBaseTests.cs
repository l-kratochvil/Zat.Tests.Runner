namespace Zat.Tests.Runner.WebApp.Tests.Shared.ViewModel;

using FluentValidation;

using NUnit.Framework;

using Zat.Tests.Runner.WebApp.Shared.ViewModel;

/// <summary>
/// What a view model does with an edit: what it takes, what it turns down, and what it says either
/// way.
/// </summary>
/// <remarks>
/// A view model that turns an edit down leaves the value as it was, so the only thing that can send
/// a control back to showing what is held is what the view model says about it. That is what most of
/// this asks about.
/// </remarks>
[TestFixture]
public class ViewModelBaseTests
{
    private const string GivenLetters = "chosen";
    private const string GivenNonsense = "1234";

    [Test]
    public void SetProperty__WhenNothingIsWrongWithEdit__ThenShouldHoldIt()
    {
        // Given:
        EditedModel unit = new();

        // When:
        unit.Name = GivenLetters;

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(unit.Name, Is.EqualTo(GivenLetters));
            Assert.That(unit.HasErrors, Is.False);
        }
    }

    [Test]
    public void SetProperty__WhenEditIsWrong__ThenShouldGoOnHoldingWhatItHad()
    {
        // Given:
        EditedModel unit = new() { Name = GivenLetters };

        // When:
        unit.Name = GivenNonsense;

        // Then:
        Assert.That(unit.Name, Is.EqualTo(GivenLetters));
    }

    [Test]
    public void SetProperty__WhenEditIsWrong__ThenShouldSayWhatIsWrongWithIt()
    {
        // Given:
        EditedModel unit = new() { Name = GivenLetters };

        // When:
        unit.Name = GivenNonsense;

        // Then:
        // What is wrong is said about the edit that was turned down, because that is the thing the
        // tester has to be told about.
        using (Assert.EnterMultipleScope())
        {
            Assert.That(unit.HasErrors, Is.True);
            Assert.That(unit.GetValidity(nameof(EditedModel.Name))?.Issues, Has.Exactly(1).Items);
        }
    }

    [Test]
    public void SetProperty__WhenEditIsWrong__ThenShouldSayPropertyMovedAllSame()
    {
        // Given:
        EditedModel unit = new() { Name = GivenLetters };

        List<string?> announced = [];
        unit.PropertyChanged += (_, args) => announced.Add(args.PropertyName);

        // When:
        unit.Name = GivenNonsense;

        // Then:
        // The value ends up where it started, so without this a control showing the turned-down
        // edit would never be told to go back to showing what is held.
        Assert.That(announced, Has.Exactly(2).EqualTo(nameof(EditedModel.Name)));
    }

    [Test]
    public void SetProperty__WhenWrongEditIsFollowedByRightOne__ThenShouldHoldRightOne()
    {
        // Given:
        // A property that starts out wrong is the ordinary case — a configuration nobody has filled
        // in yet — so being turned down once must not be what keeps it wrong.
        EditedModel unit = new();

        // When:
        unit.Name = GivenNonsense;
        unit.Name = GivenLetters;

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(unit.Name, Is.EqualTo(GivenLetters));
            Assert.That(unit.HasErrors, Is.False);
        }
    }

    [Test]
    public void SetProperty__WhenEditIsWrong__ThenShouldTakeItBackOutOfWhereverItIsKept()
    {
        // Given:
        // Where the value is kept is the caller's to say, and a view model that hands it on must not
        // leave a turned-down edit behind there.
        var kept = string.Empty;
        DispatchingModel unit = new(value => kept = value) { Name = GivenLetters };

        // When:
        unit.Name = GivenNonsense;

        // Then:
        Assert.That(kept, Is.EqualTo(GivenLetters));
    }

    [Test]
    public void InitValidator__WhenHeldValueIsWrong__ThenShouldSayWhatIsWrongWithItRightAway()
    {
        // When:
        // A configuration nobody has filled in yet is wrong before anything is edited, and the
        // tester has to see it so as not to run with it.
        EditedModel unit = new(GivenNonsense);

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(unit.HasErrors, Is.True);
            Assert.That(unit.GetValidity(nameof(EditedModel.Name))?.Issues, Has.Exactly(1).Items);
        }
    }

    [Test]
    public void InitValidator__WhenHeldValueIsRight__ThenShouldSayNothingIsWrong()
    {
        // When:
        EditedModel unit = new(GivenLetters);

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(unit.HasErrors, Is.False);
            Assert.That(unit.GetValidity(nameof(EditedModel.Name))?.Issues, Is.Empty);
        }
    }

    [Test]
    public void InitValidator__WhenHeldValueIsWrong__ThenShouldTellWhoeverHandsItOn()
    {
        // Given:
        List<bool> announced = [];

        // When:
        _ = new EditedModel(GivenNonsense, hasErrorsChanged: announced.Add);

        // Then:
        Assert.That(announced, Is.EqualTo(new[] { true }));
    }

    [Test]
    public void HasErrors__WhenEditIsTurnedDown__ThenShouldSaySoOnlyOnce()
    {
        // Given:
        EditedModel unit = new() { Name = GivenLetters };

        List<bool> announced = [];
        unit.HasErrorsChanged += has => announced.Add(has);

        // When:
        unit.Name = GivenNonsense;

        // Then:
        // Said when it changes rather than at every look, because whoever hears it hands it on.
        Assert.That(announced, Is.EqualTo(new[] { true }));
    }

    [Test]
    public void ChainValidation__WhenConditionIsTurnedOff__ThenShouldStopMindingChainedValue()
    {
        // Given:
        ChainedModel unit = new() { IsAsked = true, Name = GivenNonsense };

        // When:
        unit.IsAsked = false;

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(unit.GetValidity(nameof(ChainedModel.Name))?.HasErrors, Is.False);
            Assert.That(unit.HasErrors, Is.False);
        }
    }

    [Test]
    public void ChainValidation__WhenConditionIsTurnedOn__ThenShouldMindChainedValueAgain()
    {
        // Given:
        ChainedModel unit = new() { Name = GivenNonsense };

        // When:
        unit.IsAsked = true;

        // Then:
        Assert.That(unit.HasErrors, Is.True);
    }

    [Test]
    public void ChainValidation__WhenChainedValueWasNeverLookedAt__ThenShouldStillSayNothingAboutIt()
    {
        // Given:
        // A value nobody has been to yet is not a mistake anyone made, whatever it is chained to.
        ChainedModel unit = new();

        // When:
        unit.IsAsked = true;

        // Then:
        Assert.That(unit.GetValidity(nameof(ChainedModel.Name)), Is.Null);
    }

    /// <summary>
    /// A view model holding one value of its own.
    /// </summary>
    private sealed class EditedModel : ViewModelBase
    {
        private string name;

        public EditedModel(string name = "", Action<bool>? hasErrorsChanged = null)
        {
            this.name = name;

            if (hasErrorsChanged is not null)
            {
                this.HasErrorsChanged += hasErrorsChanged;
            }

            this.InitValidator(
                this,
                validator => validator
                    .RuleFor(model => model.Name)
                    .Matches("^[a-z]+$")
                    .WithMessage("Letters only."));
        }

        public string Name
        {
            get => this.name;
            set => this.SetProperty(ref this.name, value);
        }
    }

    /// <summary>
    /// A view model asking for a value only while it is told to.
    /// </summary>
    private sealed class ChainedModel : ViewModelBase
    {
        private bool isAsked;
        private string name = string.Empty;

        public ChainedModel()
            => this.InitValidator(
                this,
                validator => validator
                    .RuleFor(static model => model.Name)
                    .Matches("^[a-z]+$")
                    .WithMessage("Letters only.")
                    .When(static model => model.IsAsked),
                chains => chains
                    .Chain(rootProperty: static model => model.IsAsked, static model => model.Name));

        public bool IsAsked
        {
            get => this.isAsked;
            set => this.SetProperty(ref this.isAsked, value);
        }

        public string Name
        {
            get => this.name;
            set => this.SetProperty(ref this.name, value);
        }
    }

    /// <summary>
    /// A view model that hands every value it takes on to somebody else.
    /// </summary>
    private sealed class DispatchingModel : ViewModelBase
    {
        private readonly Action<string> dispatch;

        private string name = string.Empty;

        public DispatchingModel(Action<string> dispatch)
        {
            this.dispatch = dispatch;

            this.InitValidator(
                this,
                validator => validator
                    .RuleFor(model => model.Name)
                    .Matches("^[a-z]+$")
                    .WithMessage("Letters only."));
        }

        public string Name
        {
            get => this.name;
            set => this.SetProperty(
                this.name,
                value,
                accepted =>
                {
                    this.name = accepted;
                    this.dispatch(accepted);
                });
        }
    }
}