namespace Zat.Tests.Runner.TuiApp;

using DevKit.Core.Utils;

internal static class Paths
{
    public static class Directories
    {
        private static readonly string AppDataPath = Path.Combine(
            FileSystemUtils.GetLocalAppDataDirPath(),
            "Zat.Tests.Runner.TuiApp");

        private static readonly string LogsPath = Path.Combine(AppData, "logs");

        public static string AppData
            => FileSystemUtils.CreateDirectoryIfNotExisting(AppDataPath);

        public static string Logs
            => FileSystemUtils.CreateDirectoryIfNotExisting(LogsPath);

        public static string AutomizedTests
            => FileSystemUtils.CreateDirectoryIfNotExisting(Z2xxTests.Common.Paths.Directories.AutomizedTests);
    }

    public static class Files
    {
        public static string AppUserSettings { get; } = Path.Combine(Directories.AppData, "user-settings.json");

        public static string AppState { get; } = Path.Combine(Directories.AppData, "app-state.json");

        public static string TestAssemblyFilePath { get; } = Path.Combine(Directories.AutomizedTests, "Test libs", "Zat.Z2xxTests.dll");
    }
}