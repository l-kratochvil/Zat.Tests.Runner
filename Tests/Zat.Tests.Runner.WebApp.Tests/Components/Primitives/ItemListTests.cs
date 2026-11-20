namespace Zat.Tests.Runner.WebApp.Tests.Components.Primitives;

using AngleSharp.Dom;

using Bunit;

using CSharpFunctionalExtensions;

using NUnit.Framework;

using Zat.Tests.Runner.WebApp.Components.Primitives;

/// <summary>
/// Which actions a list offers over its items.
/// </summary>
[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class ItemListTests : Bunit.TestContext
{
    private static readonly string[] GivenItems = ["A", "B", "C"];

    [TearDown]
    public void TearDown()
        => this.Dispose();

    [Test]
    public void Render__WhenCommandsConfigured__ThenShouldShowAddAndRemove()
    {
        // When:
        var component = this.RenderComponent<List<string>>(
            [.. GivenItems],
            new ItemList.AddCommandConfig<string>(() => Task.FromResult(Maybe<string>.None)),
            new ItemList.RemoveCommandConfig());

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(component.FindAll(Html.MakeButtonSelector("[aria-label^='Remove']")), Has.Exactly(GivenItems.Length).Items);
            Assert.That(component.FindAll(Html.MakeButtonSelector(":not([aria-label])")), Has.Exactly(1).Items);
            Assert.That(component.Markup, Does
                .Contain(GivenItems[0])
                .And.Contain(GivenItems[1])
                .And.Contain(GivenItems[2]));
        }
    }

    [Test]
    public void Render__WhenNoCommandConfigured__ThenShouldOnlyList()
    {
        // When:
        var component = this.RenderComponent<List<string>>([.. GivenItems]);

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(component.FindAll(TagNames.Button), Is.Empty);
            Assert.That(component.Markup, Does
                .Contain(GivenItems[0])
                .And.Contain(GivenItems[1])
                .And.Contain(GivenItems[2]));
        }
    }

    [Test]
    public void Render__WhenItemsImmutable__ThenShouldOnlyList()
    {
        // When:
        var component = this.RenderComponent<string[]>([.. GivenItems]);

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(component.FindAll(TagNames.Button), Is.Empty);
            Assert.That(component.Markup, Does
                .Contain(GivenItems[0])
                .And.Contain(GivenItems[1])
                .And.Contain(GivenItems[2]));
        }
    }

    [Test]
    public void Render__WhenItemsImmutableWithAddCommand__ThenShouldThrow()
    {
        Assert.That(
            () => this.RenderComponent<string[]>(
                [.. GivenItems],
                addCommand: new ItemList.AddCommandConfig<string>(() => Task.FromResult(Maybe<string>.None))),
            Throws.InvalidOperationException);
    }

    [Test]
    public void Render__WhenItemsImmutableWithRemoveCommand__ThenShouldThrow()
    {
        Assert.That(
            () => this.RenderComponent<string[]>(
                [.. GivenItems],
                removeCommand: new ItemList.RemoveCommandConfig()),
            Throws.InvalidOperationException);
    }

    [Test]
    public void Render__WhenAddHidden__ThenShouldShowOnlyRemove()
    {
        // When:
        var component = this.RenderComponent<List<string>>(
            [.. GivenItems],
            new ItemList.AddCommandConfig<string>(() => Task.FromResult(Maybe<string>.None)) { IsVisible = false },
            new ItemList.RemoveCommandConfig());

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(component.FindAll(Html.MakeButtonSelector("[aria-label^='Remove']")), Has.Exactly(GivenItems.Length).Items);
            Assert.That(component.FindAll(Html.MakeButtonSelector(":not([aria-label])")), Is.Empty);
        }
    }

    [Test]
    public void Render__WhenRemoveHidden__ThenShouldShowOnlyAdd()
    {
        // When:
        var component = this.RenderComponent<List<string>>(
            [.. GivenItems],
            new ItemList.AddCommandConfig<string>(() => Task.FromResult(Maybe<string>.None)),
            new ItemList.RemoveCommandConfig { IsVisible = false });

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(component.FindAll(Html.MakeButtonSelector("[aria-label^='Remove']")), Is.Empty);
            Assert.That(component.FindAll(Html.MakeButtonSelector(":not([aria-label])")), Has.Exactly(1).Items);
        }
    }

    private IRenderedComponent<ItemList<string, TCollection>> RenderComponent<TCollection>(
        TCollection items,
        ItemList.AddCommandConfig<string>? addCommand = null,
        ItemList.RemoveCommandConfig? removeCommand = null)
        where TCollection : IList<string>
        => this.RenderComponent<ItemList<string, TCollection>>(
            parameters => parameters
                .Add(list => list.Items, items)
                .Add(list => list.AddCommand, addCommand)
                .Add(list => list.RemoveCommand, removeCommand));
}