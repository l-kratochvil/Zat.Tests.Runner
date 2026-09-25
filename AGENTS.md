# Agent skills

For extra context see `CONTEXT.md`.

Also include `AGENTS-SHARED.md` — check whether the file exists first; if glob finds nothing, try reading the file directly (it may be a symlink, which glob skips). If it still can't be read, tell the user.

## Issue tracker

Read `docs/agents/issue-tracker.md` before creating, reading, commenting on, labelling, or closing
an issue or PR.

## Triage labels

Read `docs/agents/triage-labels.md` before applying a triage label — it holds the canonical label
strings.

## Domain docs

Domain knowledge lives in `CONTEXT.md` at the repo root. Read
`docs/agents/domain.md` before exploring the codebase, and when writing a glossary term or an ADR.

## Code changes

### Commit subject

Write the subject in format `< prefix > - < project-name > - < file-name >: < imperative-summary >`:

- **< prefix >**: Is required. Supports one of these options: a. `Feat`: for a new capability; b. `Fix`: for a defect; c. `Change`: for reworked behaviour; d. `Dev`: for work that leaves behaviour intact (refactoring, docs, warnings, tooling)
- **< project-name >**: It's the project name minus the `Zat.SystemTest.` base prefix
  (`Zat.SystemTest.Common.Net` → `Common.Net`); drop this part when the change spans the solution.
- **< file-name >**: Name the single edited production file; use it when the change touches that one
  production file and the tests it drove, drop this part otherwise.

**Example**

```text
Fix - App - NUnitZat.Tests.RunnerProxyConnector.cs: Fix hang on proxy process exit
Feat - WebApp - TestExplorer.razor: Add test suite filter to discovery tree
Dev: Update agents context
```
