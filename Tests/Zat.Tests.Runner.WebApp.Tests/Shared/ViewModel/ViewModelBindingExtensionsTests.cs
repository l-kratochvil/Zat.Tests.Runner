namespace Zat.Tests.Runner.WebApp.Tests.Shared.ViewModel;

using Fluxor;

using Moq;

using NUnit.Framework;

using Zat.Tests.Runner.WebApp.Shared.ViewModel;

/// <summary>
/// How a view model property drawn from a Fluxor state follows it, and hands edits back to it.
/// </summary>
[TestFixture]
public class ViewModelBindingExtensionsTests
{
    private const string GivenName = "chosen";

    private BoundState state;
    private Mock<IState<BoundState>> stateMock;
    private List<string?> written;
    private List<string?> announced;
    private BoundViewModel unit;

    [SetUp]
    public void SetUp()
    {
        this.state = new BoundState(Name: null, Count: 0);

        this.stateMock = new Mock<IState<BoundState>>();
        this.stateMock.SetupGet(state => state.Value).Returns(() => this.state);

        this.written = [];
        this.announced = [];

        this.unit = new BoundViewModel(this.stateMock.Object, writeValue: this.written.Add);
        this.unit.PropertyChanged += (_, e) => this.announced.Add(e.PropertyName);
    }

    [TearDown]
    public void TearDown()
        => this.unit.Dispose();

    [Test]
    public void BindToState__WhenTheBoundValueChanges__ThenShouldAnnounceTheProperty()
    {
        // When:
        this.ChangeStateTo(this.state with { Name = GivenName });

        // Then:
        Assert.That(this.announced, Is.EqualTo(new[] { nameof(BoundViewModel.Name) }));
    }

    [Test]
    public void BindToState__WhenAnotherValueOfTheStateChanges__ThenShouldNotAnnounceTheProperty()
    {
        // When:
        this.ChangeStateTo(this.state with { Count = 1 });

        // Then:
        Assert.That(this.announced, Is.Empty);
    }

    [Test]
    public void BindToState__WhenTheBindingIsDisposed__ThenShouldStopAnnouncing()
    {
        // Given:
        this.unit.Dispose();

        // When:
        this.ChangeStateTo(this.state with { Name = GivenName });

        // Then:
        Assert.That(this.announced, Is.Empty);
    }

    [Test]
    public void BindToState__WhenTheExpressionIsNotAProperty__ThenShouldRefuseIt()
    {
        // When:
        void Bind()
            => this.unit.BindToState(this.stateMock.Object, s => s.Count, vm => vm.Name!.Length);

        // Then:
        Assert.That(Bind, Throws.ArgumentException);
    }

    [Test]
    public void Value__WhenRead__ThenShouldReturnWhatTheStateHoldsNow()
    {
        // Given:
        // Read through rather than remembered, so even a change nobody announced is what is shown.
        this.state = this.state with { Name = GivenName };

        // When:
        var name = this.unit.Name;

        // Then:
        Assert.That(name, Is.EqualTo(GivenName));
    }

    [Test]
    public void Value__WhenWritten__ThenShouldHandItOverToTheState()
    {
        // When:
        this.unit.Name = GivenName;

        // Then:
        Assert.That(this.written, Is.EqualTo(new[] { GivenName }));
    }

    [Test]
    public void Value__WhenWrittenWithWhatTheStateHolds__ThenShouldNotHandItOver()
    {
        // Given:
        this.state = this.state with { Name = GivenName };

        // When:
        this.unit.Name = GivenName;

        // Then:
        Assert.That(this.written, Is.Empty);
    }

    [Test]
    public void Value__WhenTheBindingIsReadOnly__ThenShouldRefuseTheWrite()
    {
        // Given:
        using var readOnly = new BoundViewModel(this.stateMock.Object, writeValue: null);

        // When:
        void Write()
            => readOnly.Name = GivenName;

        // Then:
        Assert.That(Write, Throws.InvalidOperationException);
    }

    private void ChangeStateTo(BoundState next)
    {
        this.state = next;
        this.stateMock.Raise(state => state.StateChanged += null, EventArgs.Empty);
    }

    // Public, because the state mock is proxied by Castle, which cannot see a private type.
    public sealed record BoundState(string? Name, int Count);

    private sealed class BoundViewModel : ViewModelBase, IDisposable
    {
        private readonly StateBinding<string?> name;

        public BoundViewModel(IState<BoundState> state, Action<string?>? writeValue)
            => this.name = this.BindToState(
                state,
                selectValue: s => s.Name,
                property: vm => vm.Name,
                writeValue: writeValue);

        public string? Name
        {
            get => this.name.Value;
            set => this.name.Value = value;
        }

        public void Dispose()
            => this.name.Dispose();
    }
}