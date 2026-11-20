namespace Zat.Tests.Runner.WebApp.Shared.JsInterop;

using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

/// <summary>
/// Creates <see cref="JsModuleInterop"/> instances for the current circuit.
/// </summary>
/// <remarks>
/// Scoped because <see cref="IJSRuntime"/> is scoped; a singleton would capture one circuit's
/// runtime and route later tabs through the wrong browser session.
/// </remarks>
/// <param name="js">JavaScript runtime of the current circuit.</param>
/// <param name="logger">Logger passed to each created <see cref="JsModuleInterop"/>.</param>
public sealed class JsModuleInteropFactory(
    IJSRuntime js,
    ILogger<JsModuleInterop> logger) : IJsModuleInteropFactory
{
    /// <inheritdoc/>
    public JsModuleInterop Create(string modulePath)
        => new(js, modulePath, logger);
}