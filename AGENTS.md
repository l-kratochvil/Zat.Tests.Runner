# Agent skills

If more context is needed see [CONTEXT.md](`CONTEXT.md`).

## Issue tracker

Before creating, reading, commenting on, labelling, assigning or closing an issue or a PR, run
`/manage-github-issues` with repo `l-kratochvil/Zat.Tests.Runner`.

### Matt Pocock skills

These skills speak of an issue tracker; map their wording onto the operations of `/manage-github-issues`:

- **"Publish to the issue tracker"**: create an issue.
- **"Fetch the relevant ticket"**: read the issue with its comments.
- **PRs as a request surface: no.** `/triage` handles issues only, never external PRs.

#### Wayfinding

`/wayfinder` keeps its **map** as one issue with **child** issues as tickets:

- **Map**: an issue labelled `wayfinder:map`, its body holding the Notes / Decisions-so-far / Fog sections.
- **Child ticket**: an issue labelled `wayfinder:<type>` (`research`/`prototype`/`grilling`/`task`), made a sub-issue of the map.
- **Blocking**: mark the child as blocked by each blocker; it is unblocked once every blocker is closed.
- **Frontier**: the map's ready sub-issues; the first one wins.
- **Claim**: assign the ticket to `@me` — the session's first write.
- **Resolve**: comment the answer on the ticket, close it, then append a context pointer (gist + link) to the map's Decisions-so-far.

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

**Example:**

```text
Fix - App - NUnitZat.Tests.RunnerProxyConnector.cs: Fix hang on proxy process exit
Feat - WebApp - TestExplorer.razor: Add test suite filter to discovery tree
Dev: Update agents context
```
