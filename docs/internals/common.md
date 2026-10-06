# Internals: Common

The mechanisms shared by `Zat.Tests.Runner.TuiApp` and `Zat.Tests.Runner.WebApp` that are not
obvious from the code.

## Contents

- [The NUnit test runner proxy](#the-nunit-test-runner-proxy)
- [One test run at a time](#one-test-run-at-a-time)
- [The test tree follows the test assembly](#the-test-tree-follows-the-test-assembly)
- [Each test assembly runs in its own AppDomain](#each-test-assembly-runs-in-its-own-appdomain)

## The NUnit test runner proxy

The purpose of `Zat.Tests.Runner.NUnitTestRunnerProxy` is to discover and run the tests on behalf of
the applications. The applications target .NET, but the test assemblies target .NET Framework,
which a .NET process cannot load; the proxy is therefore a separate .NET Framework process the
application talks to over RPC.

### How the application talks to it

The application creates a uniquely named pipe, launches the proxy and passes it the pipe name as the
first argument. The proxy connects as the client and serves `INUnitTestRunnerProxy` over
StreamJsonRpc. A proxy is launched only for as long as it is needed, one for discovery and one for
each test run, and ending the connection kills its whole process tree. A stuck test case can't
outlive the test run that way: a stopped test run that doesn't give in within a few seconds is
ended by ending its connection. The build copies the proxy into the application's output directory
(the Exchange targets in `Directory.Build.*`) so the application can launch it; a missing proxy
stops the application at start-up.

### Each test assembly runs in its own AppDomain

The proxy loads a test assembly into an AppDomain set up as if the test assembly were the
application: its directory is the application base and its `.dll.config` the configuration file.
Only so do the binding redirects and settings of the test assembly apply; in the proxy's own
AppDomain the proxy's `.exe.config` would be used instead. Loading another test assembly unloads the
AppDomain of the previous one.

### NUnit version matches the test assemblies

The `NUnit` package of `Zat.Tests.Runner.NUnitTestRunnerProxy` should be at the same version the
test assemblies reference. The proxy runs tests in-process, so a test assembly binds to the proxy's
`nunit.framework`; a mismatch may lead to problems when loading the tested assembly.

## One test run at a time

Only one test run may be in progress on the environment, because the test station it runs against
can't be shared. `ITestRunnerEngine` is therefore a singleton that refuses a second start, and the
state of its test run is shared by every client: any of them sees it and may stop it.

## The test tree follows the test assembly

The test assembly may be deployed again while the application runs, so `ITestTreeStore` discovers
the test tree again once its directory stays unchanged for a moment. When the new test tree holds
other test entities, every test selection is cleared, as it may no longer be what the user chose.
A test run still reads the test assembly on its own, and refuses to start when it no longer holds
the test selection, because NUnit would quietly skip what is missing.
