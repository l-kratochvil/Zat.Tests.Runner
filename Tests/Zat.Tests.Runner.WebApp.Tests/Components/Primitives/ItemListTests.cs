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
    public void Render__WhenItemsMutable__ThenShouldShowAddAndRemove()
    {
        // When:
        var component = this.RenderList<List<string>>([.. GivenItems]);

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
    public void Render__WhenItemsImmutable__ThenShouldNotShowAddOrRemove()
    {
        // Given:
        // An array takes no new items nor gives any up, so offering either would only end in an exception.

        // When:
        var component = this.RenderList<string[]>([.. GivenItems]);

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
    public void Render__WhenAddHidden__ThenShouldShowOnlyRemove()
    {
        // When:
        var component = this.RenderList<List<string>>([.. GivenItems], isAddVisible: false);

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
        var component = this.RenderList<List<string>>([.. GivenItems], isRemoveVisible: false);

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(component.FindAll(Html.MakeButtonSelector("[aria-label^='Remove']")), Is.Empty);
            Assert.That(component.FindAll(Html.MakeButtonSelector(":not([aria-label])")), Has.Exactly(1).Items);
        }
    }

    private IRenderedComponent<ItemList<string, TCollection>> RenderList<TCollection>(
        TCollection items,
        bool isAddVisible = true,
        bool isRemoveVisible = true)
        where TCollection : IList<string>
        => this.RenderComponent<ItemList<string, TCollection>>(
            parameters => parameters
                .Add(list => list.Items, items)
                .Add(list => list.IsAddVisible, isAddVisible)
                .Add(list => list.IsRemoveVisible, isRemoveVisible)
                .Add(list => list.CreateItem, () => Task.FromResult(Maybe<string>.None)));
}