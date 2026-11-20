# Internals: Common

The mechanisms shared by `Zat.Tests.Runner.TuiApp` and `Zat.Tests.Runner.WebApp` that are not
obvious from the code.

## Contents

- [The NUnit test runner proxy](#the-nunit-test-runner-proxy)
- [One test run at a time](#one-test-run-at-a-time)
- [The test tree follows the test assembly](#the-test-tree-follows-the-test-assembly)
- [Each test assembly runs in its own AppDomain](#each-test-assembly-runs-in-its-own-appdomain)
- [Environment](#environment)
- [Secrets](#secrets)

## The NUnit test runner proxy

The purpose of `Zat.Tests.Runner.NUnitTestRunnerProxy` is to discover and run the tests on behalf of
the applications. The applications target .NET, but the test assemblies target .NET Framework,
which a .NET process cannot load; the proxy is therefore a separate .NET Framework process the
application talks to over RPC.

### How the application talks to it

The application creates a uniquely named pipe, launches the proxy and passes it the pipe name as the
first argument. The proxy connects as the client and serves `INUnitTestRunnerProxy` over
StreamJsonRpc. A proxy is launched only for as long as it is needed, one for discovery and one for
each sub-run, and ending the connection kills its whole process tree. Running each sub-run in its own
proxy process starts static and native/COM state in the test assembly clean every time, and a stuck
test case can't outlive the sub-run: a stopped test run that doesn't give in within a few seconds is
ended by ending its connection. The build copies the proxy into the application's output directory
(the Exchange targets in `Directory.Build.*`) so the application can launch it; a missing proxy
stops the application at start-up.

### Each test assembly runs in its own AppDomain

The proxy loads a test assembly into an AppDomain set up as if the test assembly were the
application: its directory is the application base and its `.dll.config` the configuration file.
Only so do the binding redirects and settings of the test assembly apply; in the proxy's own
AppDomain the proxy's `.exe.config` would be used instead. Each discovery and each run creates its
own AppDomain and disposes it when finished; if UI automation or COM threads started by the tests
block `AppDomain.Unload` with `CannotUnloadAppDomainException`, the failure is ignored because ending
the sub-run's proxy connection kills the process anyway.

### NUnit version matches the test assemblies

The `NUnit` package of `Zat.Tests.Runner.NUnitTestRunnerProxy` should be at the same version the
test assemblies reference. The proxy runs tests in-process, so a test assembly binds to the proxy's
`nunit.framework`; a mismatch may lead to problems when loading the tested assembly.

## One test run at a time

Only one test run may be in progress on the environment, because the test station it runs against
can't be shared. `ITestRunnerEngine` is therefore a singleton that refuses a second start, and the
state of its test run is shared by every client: any of them sees it and may stop it.

The engine records the start and end of each sub-run in its `TestResult`, which also owns the shared
selection of entities reported by the front-ends.

## The test tree follows the test assembly

The test assembly may be deployed again while the application runs, so `IExploredTestAssemblyStore` discovers the test tree again once its directory stays unchanged for a moment. When the new test tree holds other test entities, every test selection is cleared, as it may no longer be what the user chose. A test run still reads the test assembly on its own, and refuses to start when it no longer holds the test selection, because NUnit would quietly skip what is missing.

## Environment

Both applications run in either `Development` or `Production`, which selects the `appsettings.<Environment>.json` overlay and controls whether user secrets are loaded. When neither `DOTNET_ENVIRONMENT` / `ASPNETCORE_ENVIRONMENT` nor `--environment` is passed, `Debug` builds default to `Development` (`AppConfiguration.UseDevelopmentWhenDebug`) and `Release` builds fall back to `Production`, so the build configuration picks the matching environment even outside an IDE.

## Secrets

The secrets (`SmtpAuthSecrets`, `TestLinkSecrets`) are options bound from the application configuration, so where they come from depends on the [environment](#environment):

- **User secrets** are read only in `Development`. Both applications share one store, as `UserSecretsId` is set for the whole solution in `Directory.Build.props`.
- **Environment variables** are read in every environment and take precedence over user secrets. Only the variables prefixed with `AppConfiguration.EnvironmentVariablesPrefix` are meant for the applications; the prefix is stripped and `__` stands for the `:` key separator.

A missing secret reads as an empty string and does not stop the start-up.
