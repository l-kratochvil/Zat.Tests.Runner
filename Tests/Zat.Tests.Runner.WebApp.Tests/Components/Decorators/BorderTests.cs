namespace Zat.Tests.Runner.WebApp.Tests.Components.Decorators;

using Bunit;

using NUnit.Framework;

using Zat.Tests.Runner.WebApp.Components.Decorators;

/// <summary>
/// How a border frames the content that belongs together.
/// </summary>
[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class BorderTests : Bunit.TestContext
{
    [TearDown]
    public void TearDown()
        => this.Dispose();

    [Test]
    public void Render__WhenGivenContent__ThenShouldWrapItInBorderElement()
    {
        // When:
        var component = this.RenderComponent<Border>(
            parameters => parameters.AddChildContent("<span class=\"content\"></span>"));

        // Then:
        Assert.That(component.FindAll(".content-frame > .content"), Has.Exactly(1).Items);
    }

    [Test]
    public void Render__WhenGivenClass__ThenShouldAddItToBorderElement()
    {
        // When:
        var component = this.RenderComponent<Border>(
            parameters => parameters
                .Add(border => border.Class, "report-group")
                .AddChildContent(string.Empty));

        // Then:
        Assert.That(component.FindAll(".content-frame.report-group"), Has.Exactly(1).Items);
    }
}