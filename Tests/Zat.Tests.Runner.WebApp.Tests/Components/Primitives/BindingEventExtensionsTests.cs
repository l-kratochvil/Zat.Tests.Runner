namespace Zat.Tests.Runner.WebApp.Tests.Components.Primitives;

using NUnit.Framework;

using Zat.Tests.Runner.WebApp.Components.Primitives;

/// <summary>
/// The one place a binding's event can be misspelled.
/// </summary>
[TestFixture]
public class BindingEventExtensionsTests
{
    [TestCase(BindingEvent.OnChange, "onchange")]
    [TestCase(BindingEvent.OnInput, "oninput")]
    public void ToEventName__WhenOfferedEventIsNamed__ThenShouldSpellItAsBrowserDoes(
        BindingEvent givenEvent, string expectedName)
    {
        // When:
        var name = givenEvent.ToEventName();

        // Then:
        Assert.That(name, Is.EqualTo(expectedName));
    }

    [Test]
    public void ToEventName__WhenEventIsNoneOfOffered__ThenShouldThrow()

        // Nothing checks the name a binding is given, so a value from outside the offer would
        // otherwise show up as a control the tester's edits never reach.
        => Assert.That(
            () => ((BindingEvent)(-1)).ToEventName(),
            Throws.InstanceOf<ArgumentOutOfRangeException>());
}
