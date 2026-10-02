# Internals: WebApp

`Zat.Tests.Runner.WebApp` is a Blazor Server front-end that runs on the test machine itself and is
reached from a browser. Even though it can be reached remotely, only one test run can execute at a
time.

The mechanisms of `Zat.Tests.Runner.WebApp` that are not obvious from the code.

## Contents

- [TypeScript/JavaScript orchestration](#typescriptjavascript-orchestration)
- [JavaScript interop](#javascript-interop)
- [The browser bridge](#the-browser-bridge)
- [Resizable panes](#resizable-panes)

## TypeScript/JavaScript orchestration

Every `{Component}.razor.ts` and every script under `Browser/` is TypeScript, compiled to
JavaScript before the browser sees it.

- **`tsconfig.json`** compiles each `.razor.ts` in place, next to its component.
- **`Browser/tsconfig.json`** compiles the shared modules under `Browser/` into `wwwroot/browser`
  instead. It lives in that folder, under that name, because the editor's language service only
  looks for `tsconfig.json` when it decides which project an open file belongs to.
- **`npm run build`** runs both compilations for both tsconfig files.
- **The `CompileTypeScript` MSBuild target** runs that npm script as part of every `dotnet build`,
  so the front-end compiles automatically.

## JavaScript interop

Components that need browser behaviour keep it in a
collocated `.razor.js` module. `JsModuleInterop` wraps one such module so a component can call into
it safely, and `IJsModuleInteropFactory` builds those wrappers.

### Calls never throw

`InvokeVoidSafeAsync` is the only way to call into a module. It reports failures instead of
propagating them.

### Why the factory is scoped

`JsModuleInteropFactory` is registered as **scoped** - that follows from `IJSRuntime`, which the framework registers as scoped.

In Blazor Server a scope is **one circuit**: one open browser tab, from the moment it connects until
it goes away. The `IJSRuntime` resolved inside a scope is the one wired to that particular tab. Components are resolved
from the same scope, so a scoped factory is handed exactly the runtime the component would have
injected itself.

The wrappers themselves are **not** registered. A component asks the factory for one, owns it, and
disposes it, following the pattern in the

## The browser bridge

`JsModuleInterop` above carries calls one way: .NET asks the browser to do something. The bridge is
the way back: it gives JS runtime code a logger shared with the application's own, so a script's
browser diagnostics land in the same log file.

A module asks for a logger once, naming itself:

```ts
import { createLogger } from "/browser/logging.js";

const log = createLogger(import.meta.url);
```

### Lifetime

`JsBridge` renders nothing; it sits in `MainLayout` for its lifetime alone, which gives exactly one
bridge per circuit and — through `Routes.razor`'s `DefaultLayout` — covers every routed page,
`/Error` included. `BrowserLogger` is a singleton: it holds a logger and no circuit of its own, so
one instance serves every browser, and each circuit merely wraps it in its own
`DotNetObjectReference`.

## Resizable panes

`SplitterBar` writes the size it is dragged to into a CSS variable on the document root, and the
stylesheet decides what that size means. Dragging therefore never touches a pane directly, and on
Blazor Server it never leaves the browser: a `pointermove` answered over SignalR would leave the
bar lagging behind the cursor.

### The layout a handle expects

A handle must sit between a pane sized by its variable and a pane that takes whatever is left
(`flex: 1 1 0`). The flexible pane absorbs every change, so a drag moves only the two panes the
handle divides and no pane further away. `TestRunnerPage` therefore sizes the two outer panes and
lets the configurator between them absorb; `MainLayout` sizes the log pane and lets the page above
it absorb.

That arrangement is also how the script knows which way to move: the pane it sizes is the
neighbour that does not grow, and the side it sits on says whether its size follows the pointer or
runs against it. The layout already states this, so the component is not told it a second time.
