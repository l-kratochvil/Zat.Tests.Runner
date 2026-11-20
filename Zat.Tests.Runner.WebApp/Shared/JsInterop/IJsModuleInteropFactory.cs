namespace Zat.Tests.Runner.WebApp.Shared.JsInterop;

/// <summary>
/// Creates <see cref="JsModuleInterop"/> wrappers.
/// </summary>
/// <remarks>
/// The factory centralizes <see cref="Microsoft.JSInterop.IJSRuntime"/> and logger wiring, so
/// callers only name a module path.
/// </remarks>
public interface IJsModuleInteropFactory
{
    /// <summary>
    /// Creates a <see cref="JsModuleInterop"/> for <paramref name="modulePath"/>.
    /// </summary>
    /// <param name="modulePath">
    /// Module path, either relative to the web root for a collocated script or absolute for a
    /// shared module.
    /// </param>
    /// <returns>A wrapper owned and disposed by the caller.</returns>
    JsModuleInterop Create(string modulePath);
}