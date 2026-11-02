namespace Zat.Tests.Runner.WebApp.Tests.Shared.ViewModel;

using FluentValidation;

using NUnit.Framework;

using Zat.Tests.Runner.WebApp.Shared.ViewModel;

/// <summary>
/// What a view model does with an edit: what it holds and what it says about it.
/// </summary>
/// <remarks>
/// A view model holds every edit, wrong or not, and says what is wrong with it; whether a wrong value
/// is handed on is for whoever reads the view model to decide.
/// </remarks>
[TestFixture]
public class ViewModelBaseTests
{
    private const string GivenLetters = "chosen";
    private const string GivenNonsense = "1234";

    [Test]
    public void SetProperty__WhenNothingIsWrongWithEdit__ThenShouldHoldIt()
    {
        // Given
        var unit = new EditedModel { Name = GivenLetters };

        // When: None

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(unit.Name, Is.EqualTo(GivenLetters));
            Assert.That(unit.HasErrors, Is.False);
        }
    }

    [Test]
    public void SetProperty__WhenEditIsWrong__ThenShouldHoldItAllSame()
    {
        // Given:
        var unit = new EditedModel { Name = GivenLetters };

        // When:
        unit.Name = GivenNonsense;

        // Then:
        Assert.That(unit.Name, Is.EqualTo(GivenNonsense));
    }

    [Test]
    public void SetProperty__WhenEditIsWrong__ThenShouldSayWhatIsWrongWithIt()
    {
        // Given:
        var unit = new EditedModel { Name = GivenLetters };

        // When:
        unit.Name = GivenNonsense;

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(unit.HasErrors, Is.True);
            Assert.That(unit.GetValidity(nameof(EditedModel.Name))?.Issues, Has.Exactly(1).Items);
        }
    }

    [Test]
    public void SetProperty__WhenEditIsWrong__ThenShouldAnnouncePropertyChanged()
    {
        // Given:
        var unit = new EditedModel { Name = GivenLetters };

        var announced = new List<string?>();
        unit.PropertyChanged += (_, args) => announced.Add(args.PropertyName);

        // When:
        unit.Name = GivenNonsense;

        // Then:
        Assert.That(announced, Has.Exactly(1).EqualTo(nameof(EditedModel.Name)));
    }

    [Test]
    public void SetProperty__WhenWrongEditIsFollowedByRightOne__ThenShouldHoldRightOne()
    {
        // Given:
        // A property that starts out wrong is the ordinary case — a configuration nobody has filled
        // in yet — so a wrong edit must not be what keeps it wrong.
        var unit = new EditedModel { Name = GivenNonsense };

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
    public void SetProperty__WhenEditIsWrong__ThenShouldHandItOnAllSame()
    {
        // Given:
        // Where the value is kept is the caller's to say, and what is wrong with it is told
        // alongside rather than by keeping it out of there.
        var kept = string.Empty;
        var unit = new DispatchingModel(value => kept = value) { Name = GivenLetters };

        // When:
        unit.Name = GivenNonsense;

        // Then:
        Assert.That(kept, Is.EqualTo(GivenNonsense));
    }

    [Test]
    public void InitValidator__WhenHeldValueIsWrong__ThenShouldSayWhatIsWrongWithItRightAway()
    {
        // Given:
        // A configuration nobody has filled in yet is wrong before anything is edited, and the
        // tester has to see it so as not to run with it.
        var unit = new EditedModel(GivenNonsense);

        // When: None

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
        var unit = new EditedModel(GivenLetters);

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
        Assert.That(announced, Is.EqualTo([true]));
    }

    [Test]
    public void HasErrors__WhenEditIsWrong__ThenShouldSaySoOnlyOnce()
    {
        // Given:
        var unit = new EditedModel { Name = GivenLetters };

        List<bool> announced = [];
        unit.HasErrorsChanged += announced.Add;

        // When:
        unit.Name = GivenNonsense;

        // Then:
        // Said when it changes rather than at every look, because whoever hears it hands it on.
        Assert.That(announced, Is.EqualTo([true]));
    }

    [Test]
    public void ChainValidation__WhenConditionIsTurnedOff__ThenShouldStopMindingChainedValue()
    {
        // Given:
        var unit = new ChainedModel { IsAsked = true, Name = GivenNonsense };

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
    public void ChainValidation__WhenConditionIsTurnedOff__ThenShouldAnnounceAfterRecheckingChainedValue()
    {
        // Given:
        var unit = new ChainedModel { IsAsked = true, Name = GivenNonsense };
        bool? announcedValidity = null;
        unit.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(ChainedModel.IsAsked))
            {
                announcedValidity = unit.GetValidity(nameof(ChainedModel.Name))?.HasErrors;
            }
        };

        // When:
        unit.IsAsked = false;

        // Then:
        Assert.That(announcedValidity, Is.False);
    }

    [Test]
    public void ChainValidation__WhenConditionIsTurnedOn__ThenShouldMindChainedValueAgain()
    {
        // Given:
        var unit = new ChainedModel { Name = GivenNonsense, IsAsked = true };

        // When: None

        // Then:
        Assert.That(unit.HasErrors, Is.True);
    }

    [TestCase(true, false)]
    [TestCase(false, true)]
    public void ConditionalValidation__WhenConditionChangesForUneditedValue__ThenShouldUpdateItsValidity(
        bool givenIsAsked,
        bool expectedHasErrors)
    {
        // Given:
        var unit = new ChainedModel(isAsked: givenIsAsked);
        Assert.That(unit.GetValidity(nameof(ChainedModel.Name))?.HasErrors, Is.EqualTo(givenIsAsked));
        List<bool> announced = [];
        unit.HasErrorsChanged += announced.Add;

        // When:
        unit.IsAsked = !givenIsAsked;

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(unit.GetValidity(nameof(ChainedModel.Name))?.HasErrors, Is.EqualTo(expectedHasErrors));
            Assert.That(unit.HasErrors, Is.EqualTo(expectedHasErrors));
            Assert.That(announced, Is.EqualTo([expectedHasErrors]));
        }
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
        public ChainedModel(bool isAsked = false)
        {
            this.IsAsked = isAsked;

            this.InitValidator(
                this,
                validator => validator
                    .RuleFor(static model => model.Name)
                    .Matches("^[a-z]+$")
                    .WithMessage("Letters only.")
                    .When(static model => model.IsAsked));
        }

        public bool IsAsked
        {
            get;
            set => this.SetProperty(ref field, value);
        }

        public string Name
        {
            get;
            set => this.SetProperty(ref field, value);
        } = string.Empty;
    }

    /// <summary>
    /// A view model that hands every value it takes on to somebody else.
    /// </summary>
    private sealed class DispatchingModel : ViewModelBase
    {
        private readonly Action<string> dispatch;

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
            get;
            set => this.SetProperty(
                field,
                value,
                accepted =>
                {
                    field = accepted;
                    this.dispatch(accepted);
                });
        } = string.Empty;
    }
}