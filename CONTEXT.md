# Context

Shared vocabulary of this repository. Terms are listed as they must be used in code, issues and
documentation; the "not" column names synonyms that are deliberately avoided because they already
mean something else here.

## Domain glossary

| Term                    | Meaning                                                                                                                                                              | Not                    |
| ----------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------- |
| **Test suite**          | A group of test cases as defined by the test assemblies / TestLink.                                                                                                  |                        |
| **Test fixture**        | The group of test cases inside a test suite, as the test assemblies define it. The middle level of the test tree.                                                    | "test suite"           |
| **Test case**           | A single test as defined by the test assemblies / TestLink.                                                                                                          |                        |
| **Test entity**         | Anything the test tree is built from, whatever its level, see `TestEntity`. What the runner is asked to run is expressed in these.                                   | "test", "node"         |
| **Execution path**      | The name a test entity is both identified and executed by, see `TestEntity.ExecutionPath`. Identity and execution deliberately share one key.                        | "name", "id"           |
| **Test selection**      | The test cases the user picked to run. Only test cases are ever part of it; what a group looks like follows from the test cases beneath it.                          | "checked tests"        |
| **Store**               | A holder of state that outlives the component reading it: the state is read whole, replaced whole and its change announced to whoever listens, see `StoreBase`.      | "cache", "repository"  |
| **Test run**            | One execution of the selected test cases on the test machine.                                                                                                        | "test", "build"        |
| **Test result**         | The outcome of a test run (passed / failed), the thing that is uploaded to TestLink.                                                                                 | "log"                  |
| **Log**                 | The application activity feed the tester reads: what the application is doing, held in memory by `AppLoggerHub` and shown in `AppLogger`. Never the test result.     | "output", "result"     |
| **Log entry**           | One record in the log: timestamp, severity, source, message and optional detail.                                                                                     | "message", "line"      |
| **Severity**            | How serious an entry is, see `LogSeverity`. A successful outcome is reported as normal progress, there is no `Success`.                                              | "level", "log level"   |
| **Log source**          | The channel an entry belongs to. A plain string, so that entries arriving from outside can be routed without a mapping; known ones: `LogSources`.                    | "category", "logger"   |
| **Diagnostics**         | Developer-facing records written through `ILogger`. They reach the log file but never the log.                                                                       | "log", "debug log"     |
| **Browser diagnostics** | Diagnostics about what happened in the browser, as opposed to on the test machine. Written by the scripts of the front-end, never by the tester.                     | "client log", "JS log" |
| **Log file**            | The daily file on disk. A superset of the log: it also holds diagnostics and framework records, see ADR-0001.                                                        | "log", "test result"   |
| **Test station**        | The hardware station a test run is executed against.                                                                                                                 |                        |
| **Options**             | A type registered in DI and bound from configuration that configures one service.                                                                                    | "settings", "config"   |

## Decisions

For more decisions history see 'docs/ADR' folder.

## More context

If more context is needed see [README.md](README.md).
