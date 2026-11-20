namespace Zat.Tests.Runner.WebApp.Shared.JsInterop;

/// <summary>
/// One browser diagnostic.
/// </summary>
/// <remarks>
/// The fields travel together because JavaScript cannot name call arguments.
/// </remarks>
/// <param name="Level">
/// Severity as the browser spells it.
/// </param>
/// <param name="Module">Script that produced the diagnostic, or where it was caught.</param>
/// <param name="Message">Single-line diagnostic message.</param>
/// <param name="Detail">Optional multi-line diagnostic detail.</param>
public sealed record BrowserDiagnostic(string? Level, string? Module, string? Message, string? Detail);