# Blazor rules

## Contents

- [Reach for the browser last](#reach-for-the-browser-last)
- [`[Inject]` names a dependency, `@inject` hides it](#inject-names-a-dependency-inject-hides-it)
- [ViewModel suffix names a component's view model](#viewmodel-suffix-names-a-components-view-model)

## Reach for the browser last

Build a view's behaviour with Blazor itself — markup and code-behind — first, with CSS next, and
with a JS/TS module only when neither can do the job. Each step down that order trades the
compiler, the DI container and the type system for a boundary that nothing checks until it runs.

A module earns its place only where Blazor cannot reach — an API that exists in the browser alone,
or an interaction a SignalR round-trip would visibly slow (e.g. a pointer drag that has to follow the cursor frame by frame). Being quicker to write in JavaScript is not such a reason. Where a module
is warranted it does that one browser-only part and no more: the state and the decisions stay in
.NET, under the [JS/TS rules](js-ts-rules.md).

## `[Inject]` names a dependency, `@inject` hides it

A component's dependencies are declared as `[Inject]` properties in `@code`, next to `[Parameter]`
and everything else the component depends on — never as an `@inject` directive at the top of the
markup. `@inject` compiles to the same property, only spelled where the rest of the component's
dependencies are not: a component's shape is read from `@code` once, not assembled from two places
that both bind a name to a service.

## ViewModel suffix names a component's view model

The `ViewModel` suffix is reserved for the view model of a Razor component of the same base name —
`Component.razor` is the view, `ComponentViewModel` is its view model. For example,
[TestExplorer.razor](/Zat.Tests.Runner.WebApp/Features/TestDiscovery/Components/TestExplorer.razor) pairs
with [TestExplorerViewModel](/Zat.Tests.Runner.WebApp/Features/TestDiscovery/Components/TestExplorerViewModel.cs).

Other classes that hold UI state or data — a tree node, a row, a DTO shown on screen — keep their
own descriptive name instead of picking up the suffix (e.g. `TestTreeNodeData`, not
`TestTreeNodeViewModel`). That way `ViewModel` unambiguously means "the view model paired with this
view" wherever it appears, rather than "some class with UI-related data".
