# Zat.Tests.Runner

Shared vocabulary of this repository. Terms are listed as they must be used in code, issues and
documentation; the _Avoid_ lines name synonyms that are deliberately avoided because they already
mean something else here.

## Language

### Test tree

**Test suite**:
A group of test cases as defined by the test assemblies / TestLink.

**Test fixture**:
The group of test cases inside a test suite, as the test assemblies define it. The middle level of the test tree.
_Avoid_: test suite

**Test case**:
A single test as defined by the test assemblies / TestLink.

**Test entity**:
Anything the test tree is built from, whatever its level, see `TestEntity`. What the runner is asked to run is expressed in these.
_Avoid_: test, node

**Execution path**:
The name a test entity is both identified and executed by, see `TestEntity.ExecutionPath`. Identity and execution deliberately share one key.
_Avoid_: name, id

**Test selection**:
The test cases the user picked to run. Only test cases are ever part of it; what a group looks like follows from the test cases beneath it.
_Avoid_: checked tests

### Test execution

**Test run**:
One execution of the selected test cases on the test machine.
_Avoid_: test, build

**Test result**:
The outcome of a test run (passed / failed), the thing that is uploaded to TestLink.
_Avoid_: log

**Test station**:
The hardware station a test run is executed against.

### Logging

**Log**:
What the application tells the tester, fed only by `LogSources.App` - never diagnostics.
_Avoid_: output, result

**Log entry**:
One record in the log: timestamp, severity, message and optional detail.
_Avoid_: message, line

**Severity**:
How serious an entry is, see `LogSeverity`. A successful outcome is reported as normal progress, there is no `Success`.
_Avoid_: level, log level

**Log source**:
The logger category a record is written under, see `LogSources`. `App` feeds the log; every other one is diagnostics.
_Avoid_: channel

**Diagnostics**:
Developer-facing records of every category other than `LogSources.App`, including a plain `ILogger` (`LogSources.Default`) and framework records. They reach the log file, console and debug output but never the log.
_Avoid_: log, debug log

**Browser diagnostics**:
Diagnostics about what happened in the browser, as opposed to on the test machine. Written by the scripts of the front-end, never by the tester.
_Avoid_: client log, JS log

**Log file**:
The daily file on disk holding diagnostics.
_Avoid_: log, test result

### Infrastructure

**Store**:
A holder of state that outlives the component reading it: the state is read whole, replaced whole and its change announced to whoever listens, see `StoreBase`.
_Avoid_: cache, repository

**Options**:
A type registered in DI and bound from configuration that configures one service.
_Avoid_: settings, config
