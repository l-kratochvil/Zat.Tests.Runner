# JS/TS rules

Rules for writing the browser code of this repository — the TypeScript that is authored and the
JavaScript it compiles to.

## Contents

- [Guard the .NET boundary at runtime](#guard-the-net-boundary-at-runtime)

## Guard the .NET boundary at runtime

An exported function that .NET calls is external API, and the call carries no types at all: the
function is named by a string and its arguments travel as JSON. Nothing on either side checks that
the two agree — a renamed parameter, a changed component, a mistyped call site, and the browser
receives whatever was sent.

Declare each such parameter as `unknown` and name it with a `u` prefix (`uElement`, `uOptions`), so
that everywhere it is used, before the guard has run, the name itself says the value is still
unchecked. Then check it with the [guards module](../../Zat.Tests.Runner.WebApp/Browser/guards.ts)
(`/browser/guards.js`), which also reports a mismatch to the application log.

If a check fails, return immediately: `null` means the call was never valid, not a value to build
on. A function that owes a value returns whatever means "nothing happened" instead.
