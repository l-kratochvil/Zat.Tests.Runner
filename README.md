# Zat.Tests.Runner 2.0

A tool for running automated NUnit test suites against a test station and reporting the results to
TestLink.

The solution contains two applications with the same purpose: `Zat.Tests.Runner.TuiApp`, a
text-based application run in a terminal, and `Zat.Tests.Runner.WebApp`, a Blazor Server front-end.
Both run the tests through the shared NUnit test runner proxy.

## Setup

The solution uses submodules that need to me initialized by running command 'git submodule update --init --recursive'.

### Setting up DevKit.Core

**A sibling clone of DevKit.Core is required.** The solution references it by relative path, so the
two repositories must sit side by side:

```text
<parent>
├── Zat.Tests.Runner2.0
└── DevKit.Core
```

Without it the build fails on an unresolvable project reference.

## Where to read on

| Document                                  | What it is for                                                         |
| ----------------------------------------- | ---------------------------------------------------------------------- |
| [docs/ADR](docs/ADR/)                     | Decisions, why they were taken, and what they cost. Dated, not edited. |
| [docs/internals](docs/internals/index.md) | How the non-obvious mechanisms actually work today, per application.  |
| [docs/rules](docs/rules/index.md)         | Rules that code in this repository must follow, and why.               |
| [docs/specs](docs/specs/)                 | Specifications of larger features.                                     |
| [AGENTS.md](AGENTS.md)                    | Working agreements for agents contributing to this repository.         |
| [CONTEXT.md](CONTEXT.md)                  | The vocabulary of this repository: what each domain term means here.   |
