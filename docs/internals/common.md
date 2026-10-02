# Internals: Common

The mechanisms shared by `Zat.Tests.Runner.TuiApp` and `Zat.Tests.Runner.WebApp` that are not
obvious from the code.

## Contents

- [The NUnit test runner proxy](#the-nunit-test-runner-proxy)
  - [Each test assembly runs in its own AppDomain](#each-test-assembly-runs-in-its-own-appdomain)

## The NUnit test runner proxy

The purpose of `Zat.Tests.Runner.NUnitTestRunnerProxy` is to discover and run the tests on behalf of
the applications. The applications target .NET, but the test assemblies target .NET Framework,
which a .NET process cannot load; the proxy is therefore a separate .NET Framework process the
application talks to over RPC.

### How the application talks to it

The application creates a uniquely named pipe, launches the proxy and passes it the pipe name as the
first argument. The proxy connects as the client and serves `INUnitTestRunnerProxy` over
StreamJsonRpc. It lives until the application closes the connection; the application also kills it
on exit as a fallback. The build copies the proxy into the application's output directory (the
Exchange targets in `Directory.Build.*`) so the application can launch it.

### Each test assembly runs in its own AppDomain

The proxy loads a test assembly into an AppDomain set up as if the test assembly were the
application: its directory is the application base and its `.dll.config` the configuration file.
Only so do the binding redirects and settings of the test assembly apply; in the proxy's own
AppDomain the proxy's `.exe.config` would be used instead. Loading another test assembly unloads the
AppDomain of the previous one.

### NUnit version matches the test assemblies

The `NUnit` package of `Zat.Tests.Runner.NUnitTestRunnerProxy` should be at the same version the
test assemblies reference. The proxy and the test assembly share one `nunit.framework` inside the
AppDomain of the test assembly; a mismatch may lead to problems when loading the tested assembly.
