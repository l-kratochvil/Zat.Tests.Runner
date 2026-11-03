namespace NUnitTestAssembly.Net481.Nested;

using NUnit.Framework;

// WARNING: DO NOT REMOVE THIS NAMESPACE - Discovery tests rely on this nested namespace to verify fixtures below the assembly's root namespace.

/// <summary>
/// Lives in a nested namespace because the discovery tests verify namespace depth.
/// </summary>
[TestFixture]
public class NestedFixture
{
    /// <summary>
    /// A single passing test case.
    /// </summary>
    [Test]
    public void NestedTest()
    {
    }
}