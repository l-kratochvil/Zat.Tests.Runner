namespace Zat.Tests.Runner.WebApp.Features.AppSettings.Models;

/// <summary>
/// What became of the last attempt to save the application settings.
/// </summary>
/// <remarks>
/// One value rather than a flag per outcome, so the editor cannot claim to have both saved and
/// failed, and so that forgetting the last attempt is a single step.
/// </remarks>
public enum SaveStatus
{
    /// <summary>
    /// Nothing has been saved since the value in the editor was last changed.
    /// </summary>
    None,

    /// <summary>
    /// The value in the editor is what was written to the settings file.
    /// </summary>
    Saved,

    /// <summary>
    /// Writing the settings file failed, so the value in the editor is not saved yet.
    /// </summary>
    Failed,
}
