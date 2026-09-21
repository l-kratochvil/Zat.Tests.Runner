namespace Zat.Tests.Runner.WebApp.Tests.AppSettings;

using System.IO;

using Moq;

using NUnit.Framework;

using Zat.Tests.Runner.WebApp.Application.Paths;
using Zat.Tests.Runner.WebApp.Features.AppSettings.Services;
using Zat.Tests.Runner.WebApp.Shared.Logging;

[TestFixture]
public class AppSettingsStoreTests
{
    private string filePath;
    private AppSettingsStore unit;

    [SetUp]
    public void SetUp()
    {
        this.filePath = Path.Combine(
            Path.GetTempPath(),
            $"Zat.Tests.Runner-settings-{Guid.NewGuid():N}",
            "user-settings.json");

        this.unit = this.CreateStore();
    }

    [TearDown]
    public void TearDown()
    {
        var directoryPath = Path.GetDirectoryName(this.filePath);
        if (directoryPath is null)
        {
            return;
        }

        if (File.Exists(directoryPath))
        {
            File.Delete(directoryPath);
        }
        else if (Directory.Exists(directoryPath))
        {
            Directory.Delete(directoryPath, recursive: true);
        }
    }

    [Test]
    public void Current__WhenNothingWasEverSaved__ThenShouldHoldWhereTheInstallerPutsTheIde()
    {
        // Then:
        Assert.That(
            this.unit.Current.IdeInstallFolderPath,
            Is.EqualTo(AppSettingsStore.DefaultIdeInstallFolderPath));
    }

    [Test]
    public async Task UpdateAsync__WhenTheSettingsAreChanged__ThenShouldHandOutTheNewOnes()
    {
        // When:
        await this.unit.UpdateAsync(settings => settings with { IdeInstallFolderPath = @"D:\Ide" });

        // Then:
        Assert.That(this.unit.Current.IdeInstallFolderPath, Is.EqualTo(@"D:\Ide"));
    }

    [Test]
    public async Task UpdateAsync__WhenTheSettingsAreChanged__ThenShouldSaySo()
    {
        // Given:
        var changedCount = 0;
        this.unit.Changed += () => changedCount++;

        // When:
        await this.unit.UpdateAsync(settings => settings with { IdeInstallFolderPath = @"D:\Ide" });

        // Then:
        Assert.That(changedCount, Is.EqualTo(1));
    }

    [Test]
    public async Task StartAsync__WhenTheSettingsWereSavedBefore__ThenShouldPutThemBack()
    {
        // Given:
        // Settings describe the installation, so they outlive the run of the application that was
        // told about them.
        await this.unit.UpdateAsync(settings => settings with { IdeInstallFolderPath = @"D:\Ide" });

        var restarted = this.CreateStore();

        // When:
        await restarted.StartAsync(CancellationToken.None);

        // Then:
        Assert.That(restarted.Current.IdeInstallFolderPath, Is.EqualTo(@"D:\Ide"));
    }

    [Test]
    public async Task StartAsync__WhenNothingWasEverSaved__ThenShouldStayOnTheDefaults()
    {
        // When:
        await this.unit.StartAsync(CancellationToken.None);

        // Then:
        Assert.That(
            this.unit.Current.IdeInstallFolderPath,
            Is.EqualTo(AppSettingsStore.DefaultIdeInstallFolderPath));
    }

    [Test]
    public async Task StartAsync__WhenTheSettingsFileIsBroken__ThenShouldStartOnTheDefaultsAnyway()
    {
        // Given:
        // A tester who cannot run any test over a broken settings file is worse off than one
        // running with the defaults, which the log tells them about.
        Directory.CreateDirectory(Path.GetDirectoryName(this.filePath)!);
        await File.WriteAllTextAsync(this.filePath, "{ not json");

        // When:
        await this.unit.StartAsync(CancellationToken.None);

        // Then:
        Assert.That(
            this.unit.Current.IdeInstallFolderPath,
            Is.EqualTo(AppSettingsStore.DefaultIdeInstallFolderPath));
    }

    [Test]
    public async Task SaveAsync__WhenTheSettingsReachTheirFile__ThenShouldSaySo()
    {
        // When:
        var result = await this.unit.SaveAsync(
            settings => settings with { IdeInstallFolderPath = @"D:\Ide" });

        // Then:
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task SaveAsync__WhenTheFileCannotBeWritten__ThenShouldSaySoRatherThanClaimSuccess()
    {
        // Given:
        // A file where the folder holding the settings should be, so writing cannot work. Telling
        // the tester their settings are saved when they are not is the one answer that costs them
        // the next run.
        await File.WriteAllTextAsync(Path.GetDirectoryName(this.filePath)!, string.Empty);

        // When:
        var result = await this.unit.SaveAsync(
            settings => settings with { IdeInstallFolderPath = @"D:\Ide" });

        // Then:
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task SaveAsync__WhenTheFileCannotBeWritten__ThenShouldLeaveTheSettingsAsTheyWere()
    {
        // Given:
        // Nothing was saved, so nothing has changed: what the application says it is configured
        // with stays what a restart would find, and the tester can try again.
        await File.WriteAllTextAsync(Path.GetDirectoryName(this.filePath)!, string.Empty);

        // When:
        await this.unit.SaveAsync(settings => settings with { IdeInstallFolderPath = @"D:\Ide" });

        // Then:
        Assert.That(
            this.unit.Current.IdeInstallFolderPath,
            Is.EqualTo(AppSettingsStore.DefaultIdeInstallFolderPath));
    }

    [Test]
    public async Task SaveAsync__WhenAListenerFails__ThenShouldHaveWrittenTheFileAnyway()
    {
        // Given:
        // Everyone connected listens to this one instance, and a listener belonging to another
        // browser throwing must not cost the tester their settings.
        this.unit.Changed += () => throw new InvalidOperationException("a listener of another circuit");

        // When:
        try
        {
            await this.unit.SaveAsync(settings => settings with { IdeInstallFolderPath = @"D:\Ide" });
        }
        catch (InvalidOperationException)
        {
            // The listener's failure is not what is under test here.
        }

        // Then:
        Assert.That(await File.ReadAllTextAsync(this.filePath), Does.Contain(@"D:\\Ide"));
    }

    private AppSettingsStore CreateStore()
    {
        var paths = new Mock<IAppPathsProvider>();
        paths.SetupGet(provider => provider.Files).Returns(new AppFilePaths(this.filePath));

        return new AppSettingsStore(paths.Object, new Mock<IAppLogger>().Object);
    }
}