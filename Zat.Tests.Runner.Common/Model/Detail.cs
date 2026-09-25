namespace Zat.Tests.Runner.Common.Model;

/// <summary>
/// The detail of a <see cref="TestEntityResult"/>.
/// </summary>
/// <param name="Message">The message.</param>
/// <param name="StackTrace">The stack trace.</param>
public record Detail(
    string Message,
    string? StackTrace)
{
    /// <summary>
    /// Gets the message.
    /// </summary>
    public string Message { get; } = Message;

    /// <summary>
    /// Gets the stack trace.
    /// </summary>
    public string? StackTrace { get; } = StackTrace;
}