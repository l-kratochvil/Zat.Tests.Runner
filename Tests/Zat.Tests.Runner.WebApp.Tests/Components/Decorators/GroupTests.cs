namespace Zat.Tests.Runner.WebApp.Tests.Components.Decorators;

using Bunit;

using NUnit.Framework;

using Zat.Tests.Runner.WebApp.Components.Decorators;

/// <summary>
/// How a group frames the content that belongs together.
/// </summary>
[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class GroupTests : Bunit.TestContext
{
    [TearDown]
    public void TearDown()
        => this.Dispose();

    [Test]
    public void Render__WhenGivenContent__ThenShouldWrapItInGroupElement()
    {
        // When:
        var component = this.RenderComponent<Group>(
            parameters => parameters.AddChildContent("<span class=\"content\"></span>"));

        // Then:
        Assert.That(component.FindAll(".group > .content"), Has.Exactly(1).Items);
    }

    [Test]
    public void Render__WhenGivenClass__ThenShouldAddItToGroupElement()
    {
        // When:
        var component = this.RenderComponent<Group>(
            parameters => parameters
                .Add(group => group.Class, "report-group")
                .AddChildContent(string.Empty));

        // Then:
        Assert.That(component.FindAll(".group.report-group"), Has.Exactly(1).Items);
    }
}