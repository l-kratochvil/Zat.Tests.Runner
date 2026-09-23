namespace Zat.Tests.Runner.WebApp.Tests.Shared.Stores;

using NUnit.Framework;

using Zat.Tests.Runner.WebApp.Shared.Stores;

[TestFixture]
public class StoreBaseTests
{
    private TestStore unit;

    [SetUp]
    public void SetUp()
    {
        this.unit = new TestStore();
    }

    [Test]
    public void Current__WhenNothingHasChangedTheState__ThenShouldBeTheDefaultOne()
    {
        // Then:
        Assert.That(this.unit.Current.Value, Is.EqualTo(TestStore.DefaultValue));
    }

    [Test]
    public void Current__WhenItIsReadTwice__ThenShouldBuildTheDefaultStateOnlyOnce()
    {
        // Given:
        // The default state is built on first use rather than in the constructor, which must not
        // turn into building a new one on every read: callers compare what they hold to what the
        // store holds.
        var firstRead = this.unit.Current;

        // When:
        var secondRead = this.unit.Current;

        // Then:
        Assert.That(secondRead, Is.SameAs(firstRead));
    }

    [Test]
    public async Task UpdateAsync__WhenTheStateIsChanged__ThenShouldHandOutTheNewOne()
    {
        // Given:
        const string expectedValue = "changed";

        // When:
        await this.unit.UpdateAsync(state => state with { Value = expectedValue });

        // Then:
        Assert.That(this.unit.Current.Value, Is.EqualTo(expectedValue));
    }

    [Test]
    public async Task UpdateAsync__WhenTheStateIsChanged__ThenShouldSaySo()
    {
        // Given:
        var changedCount = 0;
        this.unit.Changed += _ => changedCount++;

        // When:
        await this.unit.UpdateAsync(state => state with { Value = "changed" });

        // Then:
        Assert.That(changedCount, Is.EqualTo(1));
    }

    [Test]
    public void Changed__WhenNobodyIsListening__ThenShouldNotStopTheUpdate()
    {
        // When, Then:
        Assert.DoesNotThrowAsync(() => this.unit.UpdateAsync(state => state with { Value = "changed" }));
    }

    private sealed record TestState(string Value);

    private sealed class TestStore : StoreBase<TestState>
    {
        public const string DefaultValue = "default";

        protected override TestState DefaultState
            => new(DefaultValue);
    }
}