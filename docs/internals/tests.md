# Test assemblies

The .NET Framework 4.6.1 and 4.8.1 NUnit test assemblies share common test cases through `NUnitTestAssembly.Common`; framework-specific behavior and fixture-level setup or teardown stay in their respective assemblies. Each assembly exposes its own name through `TestAssembly.Name`, keeping consumers independent of hard-coded assembly names.
