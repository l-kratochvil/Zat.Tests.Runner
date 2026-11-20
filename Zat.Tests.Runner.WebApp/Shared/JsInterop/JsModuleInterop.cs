namespace Zat.Tests.Runner.WebApp.Shared.JsInterop;

using System.Diagnostics;

using DevKit.Core.Extensions.Types;

using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

/// <summary>
/// Wraps calls into a JavaScript module so the module path is spelled out in exactly one place.
/// Created through <see cref="IJsModuleInteropFactory"/>, then owned and disposed by the
/// component that asked for it (it is not registered in DI itself), following the pattern in
/// https://learn.microsoft.com/aspnet/core/blazor/javascript-interoperability/.
/// </summary>
/// <remarks>
/// Calls never throw, see <see cref="InvokeVoidSafeAsync"/>. A component using JavaScript for
/// presentational behaviour cannot repair a failed call anyway, so a failure is reported to the
/// developer instead of being handed to a caller that would only swallow it.
/// </remarks>
/// <param name="js">JavaScript runtime of the circuit the calls are made on.</param>
/// <param name="modulePath">Path of the module, relative to the web root.</param>
/// <param name="logger">Logger the failures are reported to.</param>
public sealed class JsModuleInterop(
    IJSRuntime js,
    string modulePath,
    ILogger<JsModuleInterop> logger) : IAsyncDisposable
{
    private const string NoFunction = "(none)";

    private readonly Lazy<Task<IJSObjectReference>> moduleTask = new(
        () => js.InvokeAsync<IJSObjectReference>("import", modulePath).AsTask());

    /// <summary>
    /// Calls an exported function of the module that returns nothing, reporting a failure instead
    /// of throwing.
    /// </summary>
    /// <remarks>
    /// Importing the module is part of the call, so a missing or broken module is reported the
    /// same way. A failed import is remembered: every later call on this instance fails again
    /// instead of retrying, because a module that could not be imported is a build or deployment
    /// fault and not a transient outage.
    /// </remarks>
    /// <param name="identifier">Name of the exported function.</param>
    /// <param name="args">Arguments passed to the function.</param>
    /// <returns>A task that completes once the call has been made, or has failed.</returns>
    public async Task InvokeVoidSafeAsync(string identifier, params object?[]? args)
    {
        IJSObjectReference module;

        try
        {
            module = await this.moduleTask.Value;
        }
        catch (Exception exception) when (IsBrowserGone(exception))
        {
            return;
        }
        catch (Exception exception)
        {
            this.Report("import the JS module", exception, identifier: null);

            return;
        }

        try
        {
            await module.InvokeVoidAsync(identifier, args);
        }
        catch (Exception exception) when (IsBrowserGone(exception))
        {
        }
        catch (Exception exception)
        {
            this.Report("call the JS function", exception, identifier);
        }
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (!this.moduleTask.IsValueCreated)
        {
            return;
        }

        // An import that failed leaves nothing to dispose, and awaiting its task would only throw
        // the very same exception again, in the middle of tearing a component down.
        if (this.moduleTask.Value.IsFaulted)
        {
            return;
        }

        try
        {
            var module = await this.moduleTask.Value;
            await module.DisposeAsync();
        }
        catch (Exception exception) when (IsBrowserGone(exception))
        {
        }
        catch (Exception exception)
        {
            this.Report("dispose the JS module", exception, identifier: null);
        }
    }

    /// <summary>
    /// Tells the browser having gone away apart from the application having done something wrong.
    /// </summary>
    /// <remarks>
    /// Closing a tab ends the circuit mid-call, and shutting one down cancels the calls in flight.
    /// Neither is a fault, so neither may reach the log or stop the developer in the debugger.
    /// </remarks>
    /// <param name="exception">Exception the call failed with.</param>
    /// <returns><see langword="true"/> if the browser is already gone.</returns>
    private static bool IsBrowserGone(Exception exception)
        => exception is JSDisconnectedException or OperationCanceledException;

    /// <summary>
    /// Reports a failure to the developer: to the logging pipeline, and to the debugger through
    /// <c>Debug.SafeFail</c>.
    /// </summary>
    /// <remarks>
    /// This never reaches the application log the tester reads: a broken module is developer
    /// detail the tester can do nothing about.
    /// </remarks>
    /// <param name="action">What failed, worded to follow "Failed to".</param>
    /// <param name="exception">Exception the call failed with.</param>
    /// <param name="identifier">Name of the exported function, if the failure has one.</param>
    private void Report(string action, Exception exception, string? identifier)
    {
        logger.LogError(
            exception,
            "Failed to {Action}. Module: {ModulePath}, function: {Identifier}.",
            action,
            modulePath,
            identifier ?? NoFunction);

        // The message is spelled out instead of the exception being passed along, because SafeFail
        // takes only a string and the assertion dialog shows no stack trace: the exception type is
        // the first thing worth seeing there.
        Debug.SafeFail(
            $"Failed to {action}. Module: {modulePath}, function: {identifier ?? NoFunction}. "
            + $"{exception.GetType().Name}: {exception.Message}");
    }
}