# Agent skills

For extra context see `CONTEXT.md`. Also see `AGENTS-SHARED-md` if the file exists.

## Issue tracker

Read `docs/agents/issue-tracker.md` before creating, reading, commenting on, labelling, or closing
an issue or PR.

## Triage labels

Read `docs/agents/triage-labels.md` before applying a triage label — it holds the canonical label
strings.

## Writing docs

Describe the **essence** — what the thing is for and the rule it upholds — never the
implementation. Use as few sentences as the essence needs, optimally one or two. Applies to every
doc you write in this repo: markdown docs, ADRs, and code doc comments alike.

## Domain docs

Domain knowledge lives in `CONTEXT.md` at the repo root. Read
`docs/agents/domain.md` before exploring the codebase, and when writing a glossary term or an ADR.

## Code docs

Cross-reference symbols with `<paramref>`, `<typeparamref>`, and `<see cref>` instead of naming
them in prose:

```csharp
/// <returns><see cref="TResult"/> if <paramref name="value"/> can be converted to <typeparamref name="TResult"/>, otherwise <see langword="null"/>.</returns>
```

## Code changes

### Static analysis

Read the analyzer output of every build and drive it to zero: fix all analyzer diagnostics
(errors, warnings, and info-level alike) introduced or surfaced by your changes; do not fix
unrelated pre-existing diagnostics unless asked.

### Committing

Never commit on your own initiative — leave changes in the working tree until the user explicitly
asks for a commit.

#### Prefixing

Write the subject in format `< prefix > - < project-name > - < file-name >: < imperative-summary >`:

- **< prefix >**: Is required. Supports one of these options: a. `Feat`: for a new capability; b. `Fix`: for a defect; c. `Change`: for reworked behaviour; d. `Dev`: for work that leaves behaviour intact (refactoring, docs, warnings, tooling)
- **< project-name >**: It's the project name minus the `Zat.Tests.Runner.` base prefix
  (`Zat.Tests.Runner.WebApp` → `WebApp`); drop this part when the change spans the solution.
- **< file-name >**: Name the single edited file, plus its follow-up tests; drop this part otherwise.

##### Examples

```text
Fix - App - NUnitZat.Tests.RunnerProxyConnector.cs: Fix hang on proxy process exit
Feat - WebApp - TestExplorer.razor: Add test suite filter to discovery tree
Dev: Update agents context
```
