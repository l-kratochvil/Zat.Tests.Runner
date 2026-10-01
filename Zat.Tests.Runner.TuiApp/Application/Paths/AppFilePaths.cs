namespace Zat.Tests.Runner.TuiApp.Application.Paths;

using Zat.Tests.Runner.Common.Net.Application.Paths;

/// <summary>
/// The files of this application.
/// </summary>
internal sealed record AppFilePaths : SharedAppFilePaths
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AppFilePaths"/> class.
    /// </summary>
    /// <param name="shared">The files every application has.</param>
    /// <param name="appState">Full path of the file the application state is kept in.</param>
    public AppFilePaths(SharedAppFilePaths shared, string appState)
        : base(shared)
        => this.AppState = appState;

    /// <summary>
    /// Gets the full path of the file the application state is kept in.
    /// </summary>
    public string AppState { get; }
}