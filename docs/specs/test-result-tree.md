# Spec: Test result as a tree of test suites, test fixtures and test cases

## Problem Statement

When a whole test suite or test fixture fails (e.g. its `OneTimeSetUp` throws) or is blocked (ignored,
not runnable), the tester sees the same error repeated once for every test case beneath it, which
buries the real cause. At the same time TestLink needs a separate test result for every test case,
so the per-test-case outcome must not be lost. Today the proxy additionally cannot produce a test
result at all: the result collection is half-rewritten, its status mapping throws for some outcomes
(skipped/warning groups, cancelled runs), and errors raised only at a group level (e.g.
`OneTimeTearDown`) either get duplicated or disappear.

## Solution

A test run returns a tree that mirrors the test tree: test suites → test fixtures → test cases. Every
node carries its own status and optional detail (message + stack trace). A test suite or test fixture
has a non-passed status only when the problem originated on it. The tester's report prints such a
node once and does not descend into its children; otherwise it prints the problems of the children.
TestLink keeps receiving one test result per test case, including test cases whose outcome was
inherited from a failed/blocked parent.

## User Stories

1. As a tester, I want a failed `OneTimeSetUp` of a test fixture reported once, so that I immediately see the single cause instead of N identical errors.
2. As a tester, I want a failed `OneTimeSetUp` of a test suite (e.g. a set-up fixture) reported once at the test suite, so that the report stays short when the whole test suite is broken.
3. As a tester, I want an ignored test fixture reported once with its reason, so that I know why none of its test cases ran.
4. As a tester, I want a not-runnable (invalid) test fixture reported once with its reason, so that I can fix the test code quickly.
5. As a tester, I want an error in `OneTimeTearDown` of a test fixture or test suite reported, so that environment clean-up problems are not silently lost.
6. As a tester, I want test cases under a group with a teardown error still shown as passed, so that I can tell the test cases themselves succeeded.
7. As a tester, I want failures, errors, invalid and warning outcomes of individual test cases listed with message and stack trace when their parent is fine, so that I can analyse each problem.
8. As a tester, I want ignored, explicit and otherwise skipped test cases listed in the "not run" section with their reason, so that I know what was not executed.
9. As a tester, I want test cases that did not get a result (e.g. the run was cancelled) shown as unknown / not run, so that the report never pretends they passed.
10. As a tester, I want the summary counts (total, passed, failed, warnings, inconclusive, skipped and their breakdowns) computed over all test cases, so that the numbers match what TestLink receives.
11. As a tester, I want the overall status of a test run to reflect group-level problems too (e.g. teardown error), so that a run with a broken teardown is not reported as fully passed.
12. As a tester, I want to run a whole test suite or test fixture without selecting its test cases one by one, so that I can start large runs quickly — and still get a result for every test case beneath it.
13. As a tester, I want a cancelled test case reported as an error, so that an interrupted run is visibly incomplete.
14. As a TestLink uploader, I want one test result per test case, identified by the same id as the test case entity, so that the upload maps to the right TestLink test case.
15. As a TestLink uploader, I want test cases inheriting a parent's failure to be uploaded as failed/blocked with the parent's message, so that TestLink reflects that they did not pass.
16. As a TestLink uploader, I want every status mapped exactly once (no duplicate or missing mapping arms), so that no test result is uploaded with an empty status by mistake.
17. As a developer, I want the NUnit status mapping in one place with no throwing default and a fallback to unknown, so that a new or unusual NUnit outcome never crashes the test run.
18. As a developer, I want NUnit result labels referenced via NUnit's own constants instead of string literals, so that the mapping cannot drift from NUnit.
19. As a developer, I want the test case id computed by one helper shared by test discovery and result collection, so that discovered test case entities and their test results always agree.
20. As a developer, I want group and test case results to share one base shape (entity name, status, detail), so that consumers can treat any node uniformly.
21. As a developer of the web app, I want the web app to keep compiling against the new result shape, so that the solution builds.

## Implementation Decisions

- **Result model (Common)** — a tree mirroring test entities:
  - `TestEntityResult` — base record: `EntityName` (execution path of the node), `Status` (`TestStatus`), `Detail?`.
  - `Detail` — `Message`, `StackTrace`; `null` when there is nothing to say.
  - `TestSuiteResult : TestEntityResult` — holds `TestFixtureResult[]`.
  - `TestFixtureResult : TestEntityResult` — holds `TestCaseResult[]`.
  - `TestCaseResult : TestEntityResult` — adds `Id` as `string` (was `int`), same value as the test case entity id.
  - `ProxyTestResult` — holds `TestSuiteResult[]`.
  - A test suite / test fixture without an own problem has `Passed` and no detail.
- **Proxy input stays `IEnumerable<TestEntity>`**: a test suite or test fixture may be requested without its test cases being known to the caller. The filter keeps OR semantics over execution paths.
- **Result collection in the proxy**:
  - The skeleton (test suites → test fixtures → test cases) is built from the loaded test tree filtered by the same filter as the run, so requesting a group expands to its test cases and test cases missing from the NUnit result tree are still present.
  - Each node is matched to its NUnit result by full name = execution path.
  - Test suite / test fixture status comes only from its intrinsic outcome; outcomes aggregated from children (`FailureSite.Child`) or a missing result count as `Passed`.
  - Test case status is always mapped from its own NUnit result, including outcomes propagated from a parent (`FailureSite.Parent`), whose message/stack trace NUnit already carries from the parent.
  - A test case without a NUnit result gets `Unknown` with a "not run" message.
  - The collected buckets and the old validating status switch are removed.
- **NUnit → `TestStatus` mapping** (single switch, no leaf guards, labels via `ResultState.*.Label`):
  - Passed → Passed; Inconclusive → Inconclusive; Warning → Warning.
  - Failed + label Error → Error; Failed + label Invalid (not runnable) → Invalid; Failed + label Cancelled → Error; other Failed → Failure.
  - Skipped + label Ignored → Ignored; Skipped + label Explicit → Explicit; other Skipped → Skipped.
  - Anything else → Unknown.
- **Test case id**: one helper (`TestCaseAttribute.TestName`, falling back to the test name) used both by discovery and by result collection.
- **`TestResult` (Common.Net)**:
  - Per-status collections and counts are computed over the flattened test cases.
  - The removed `Other` status is replaced by `Skipped`.
  - `OverallStatus` also takes the statuses of test suites and test fixtures into account.
- **TestLink result handler**: iterates flattened test cases; the duplicate `Skipped` mapping arm is removed.
- **TuiApp report**: walks the tree; a node whose status is not `Passed` is printed once (with its detail) and its children are skipped; summary counts and overall status follow the `TestResult` changes.
- **WebApp**: only adapted to compile.

## Testing Decisions

- Good tests assert external behaviour only — the shape and values of the returned test result — never how the proxy walks NUnit internals.
- **Seam 1 (main)**: `NUnitTestRunnerProxy.RunTestAsync` over the build-time sample test assembly (NUnitTestAssembly.Net481). The sample assembly is extended with test fixtures that have a failing `OneTimeSetUp`, a failing `OneTimeTearDown`, and an ignored fixture. Tests verify the test suite → test fixture → test case tree, statuses, details, ids, group-level statuses reported once, and running a whole test suite via a `TestSuiteEntity`. The existing, currently broken `RunTestAsync` test is rewritten against the new shape.
- **Seam 2**: `TestResult` in Common.Net — pure unit tests over a hand-built `ProxyTestResult` for the flattened collections/counts and `OverallStatus` (including a group-level teardown error with all test cases passed).
- The TuiApp report has no automated tests; it is verified manually.
- Prior art: the `LoadTestAssemblyAsync_*` tests in the proxy test project (NUnit, Given-When-Then, sample assembly on disk).

## Out of Scope

- Rendering changes in the WebApp beyond compiling.
- Uploading execution attachments (screenshots) to TestLink.
- Mapping `Inconclusive`/`Warning`/`Unknown` to TestLink statuses (existing TODO).
- Mapping test suite display names in the proxy (existing TODO).
- Deciding the test type by attribute instead of name (existing TODO).

## Further Notes

- NUnit behaviour relied on: after a failed `OneTimeSetUp` (and for ignored / not-runnable fixtures) NUnit adds a result for every child with `FailureSite.Parent` and the parent's message and stack trace; after a failed `OneTimeTearDown` children keep their own results. A child result is added to its parent only when the child completes, hence the skeleton from the loaded test tree for cancelled runs.
- Pre-existing build breaks from work in progress (`TestStatus.Other` references) are resolved as part of the `TestResult` change.
