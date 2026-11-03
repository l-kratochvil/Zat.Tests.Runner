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
        var unit = new EditedViewModel { Name = GivenLetters };

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
        var unit = new EditedViewModel { Name = GivenLetters };

        // When:
        unit.Name = GivenNonsense;

        // Then:
        Assert.That(unit.Name, Is.EqualTo(GivenNonsense));
    }

    [Test]
    public void SetProperty__WhenEditIsWrong__ThenShouldSayWhatIsWrongWithIt()
    {
        // Given:
        var unit = new EditedViewModel { Name = GivenLetters };

        // When:
        unit.Name = GivenNonsense;

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(unit.HasErrors, Is.True);
            Assert.That(unit.GetValidity(nameof(EditedViewModel.Name))?.Issues, Has.Exactly(1).Items);
        }
    }

    [Test]
    public void SetProperty__WhenEditIsWrong__ThenShouldAnnouncePropertyChanged()
    {
        // Given:
        var unit = new EditedViewModel { Name = GivenLetters };

        var announced = new List<string?>();
        unit.PropertyChanged += (_, args) => announced.Add(args.PropertyName);

        // When:
        unit.Name = GivenNonsense;

        // Then:
        Assert.That(announced, Has.Exactly(1).EqualTo(nameof(EditedViewModel.Name)));
    }

    [Test]
    public void SetProperty__WhenWrongEditIsFollowedByRightOne__ThenShouldHoldRightOne()
    {
        // Given:
        // A property that starts out wrong is the ordinary case — a configuration nobody has filled
        // in yet — so a wrong edit must not be what keeps it wrong.
        var unit = new EditedViewModel { Name = GivenNonsense };

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
        var unit = new DispatchingViewModel(value => kept = value) { Name = GivenLetters };

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
        var unit = new EditedViewModel(GivenNonsense);

        // When: None

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(unit.HasErrors, Is.True);
            Assert.That(unit.GetValidity(nameof(EditedViewModel.Name))?.Issues, Has.Exactly(1).Items);
        }
    }

    [Test]
    public void InitValidator__WhenHeldValueIsRight__ThenShouldSayNothingIsWrong()
    {
        // When:
        var unit = new EditedViewModel(GivenLetters);

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(unit.HasErrors, Is.False);
            Assert.That(unit.GetValidity(nameof(EditedViewModel.Name))?.Issues, Is.Empty);
        }
    }

    [Test]
    public void InitValidator__WhenHeldValueIsWrong__ThenShouldTellWhoeverHandsItOn()
    {
        // Given:
        var announced = new List<bool>();

        // When:
        _ = new EditedViewModel(GivenNonsense, hasErrorsChanged: announced.Add);

        // Then:
        Assert.That(announced, Is.EqualTo([true]));
    }

    [Test]
    public void HasErrors__WhenEditIsWrong__ThenShouldSaySoOnlyOnce()
    {
        // Given:
        var unit = new EditedViewModel { Name = GivenLetters };

        List<bool> announced = [];
        unit.HasErrorsChanged += announced.Add;

        // When:
        unit.Name = GivenNonsense;

        // Then:
        // Said when it changes rather than at every look, because whoever hears it hands it on.
        Assert.That(announced, Is.EqualTo([true]));
    }

    [Test]
    public void ConditionalValidation__WhenConditionIsTurnedOff__ThenShouldStopValidatingName()
    {
        // Given:
        var unit = new ConditionalValidationViewModel { IsNameValidated = true, Name = GivenNonsense };

        // When:
        unit.IsNameValidated = false;

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(unit.GetValidity(nameof(ConditionalValidationViewModel.Name))?.HasErrors, Is.False);
            Assert.That(unit.HasErrors, Is.False);
        }
    }

    [Test]
    public void ConditionalValidation__WhenConditionIsTurnedOff__ThenShouldAnnounceAfterRevalidatingName()
    {
        // Given:
        var unit = new ConditionalValidationViewModel { IsNameValidated = true, Name = GivenNonsense };
        bool? announcedValidity = null;
        unit.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(ConditionalValidationViewModel.IsNameValidated))
            {
                announcedValidity = unit.GetValidity(nameof(ConditionalValidationViewModel.Name))?.HasErrors;
            }
        };

        // When:
        unit.IsNameValidated = false;

        // Then:
        Assert.That(announcedValidity, Is.False);
    }

    [Test]
    public void ConditionalValidation__WhenConditionIsTurnedOn__ThenShouldValidateName()
    {
        // Given:
        var unit = new ConditionalValidationViewModel { Name = GivenNonsense, IsNameValidated = true };

        // When: None

        // Then:
        Assert.That(unit.HasErrors, Is.True);
    }

    [TestCase(true, false)]
    [TestCase(false, true)]
    public void ConditionalValidation__WhenConditionChangesForUneditedName__ThenShouldUpdateItsValidity(
        bool givenIsNameValidated,
        bool expectedHasErrors)
    {
        // Given:
        var unit = new ConditionalValidationViewModel(isNameValidated: givenIsNameValidated);
        Assert.That(unit.GetValidity(nameof(ConditionalValidationViewModel.Name))?.HasErrors, Is.EqualTo(givenIsNameValidated));
        var announced = new List<bool>();
        unit.HasErrorsChanged += announced.Add;

        // When:
        unit.IsNameValidated = !givenIsNameValidated;

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(unit.GetValidity(nameof(ConditionalValidationViewModel.Name))?.HasErrors, Is.EqualTo(expectedHasErrors));
            Assert.That(unit.HasErrors, Is.EqualTo(expectedHasErrors));
            Assert.That(announced, Is.EqualTo([expectedHasErrors]));
        }
    }

    private static IRuleBuilderOptions<TViewModel, string> MustBeLetters<TViewModel>(
        IRuleBuilder<TViewModel, string> rule) =>
        rule
            .Matches("^[a-z]+$")
            .WithMessage("Letters only.");

    /// <summary>
    /// A view model holding one value of its own.
    /// </summary>
    private sealed class EditedViewModel : ViewModelBase
    {
        private string name;

        public EditedViewModel(string name = "", Action<bool>? hasErrorsChanged = null)
        {
            this.name = name;

            if (hasErrorsChanged is not null)
            {
                this.HasErrorsChanged += hasErrorsChanged;
            }

            this.InitValidator(
                this,
                validator => MustBeLetters(validator.RuleFor(static model => model.Name)));
        }

        public string Name
        {
            get => this.name;
            set => this.SetProperty(ref this.name, value);
        }
    }

    /// <summary>
    /// A view model validating <see cref="Name"/> only while <see cref="IsNameValidated"/> is set.
    /// </summary>
    private sealed class ConditionalValidationViewModel : ViewModelBase
    {
        public ConditionalValidationViewModel(bool isNameValidated = false)
        {
            this.IsNameValidated = isNameValidated;

            this.InitValidator(
                this,
                validator => MustBeLetters(validator.RuleFor(static model => model.Name))
                    .When(static model => model.IsNameValidated));
        }

        public bool IsNameValidated
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
    private sealed class DispatchingViewModel : ViewModelBase
    {
        private readonly Action<string> dispatch;

        public DispatchingViewModel(Action<string> dispatch)
        {
            this.dispatch = dispatch;

            this.InitValidator(
                this,
                validator => MustBeLetters(validator.RuleFor(static model => model.Name)));
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