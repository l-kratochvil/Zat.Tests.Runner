namespace Zat.Tests.Runner.WebApp.Tests.AppSettings;

using System.IO;

using Bunit;

using Microsoft.Extensions.DependencyInjection;

using Moq;

using NUnit.Framework;

using Zat.Tests.Runner.WebApp.Application.Paths;
using Zat.Tests.Runner.WebApp.Features.AppSettings.Services;
using Zat.Tests.Runner.WebApp.Shared.Logging;

using SettingsEditorComponent = Zat.Tests.Runner.WebApp.Features.AppSettings.Components.AppSettings;

/// <summary>
/// What the editor lets the tester save, and what it says about the last attempt at it.
/// </summary>
[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class SettingsEditorTests : Bunit.TestContext
{
    private const string InstallFolderSelector = ".settings-ide-install-folder";
    private const string SaveSelector = "button";
    private const string ErrorSelector = ".property-grid-row-message.is-error";
    private const string WarningSelector = ".property-grid-row-message.is-warning";
    private const string ReachableFolderPath = @"D:\Ide";
    private const string UnreachableFolderPath = @"D:\Gone";

    private string folderPath;
    private string settingsFilePath;
    private Mock<IDirectoryReader> directoryReader;

    [SetUp]
    public void SetUp()
    {
        this.folderPath = Path.Combine(
            Path.GetTempPath(), $"Zat.Tests.Runner-settings-{Guid.NewGuid():N}");

        this.directoryReader = new Mock<IDirectoryReader>();
        this.directoryReader
            .Setup(reader => reader.ReadSubFolderNames(It.IsAny<string>()))
            .Returns<string>(static _ => ["6"]);
        this.directoryReader
            .Setup(reader => reader.ReadSubFolderNames(UnreachableFolderPath))
            .Throws(new DirectoryNotFoundException());

        this.settingsFilePath = Path.Combine(this.folderPath, "settings.json");

        // Resolved when the editor is rendered, so a test can point the settings somewhere they
        // cannot be written before then.
        this.Services.AddSingleton(_ => this.CreateStore(this.settingsFilePath));
        this.Services.AddSingleton<IAppSettingsValidator>(
            new AppSettingsValidator(this.directoryReader.Object));
    }

    [TearDown]
    public void TearDown()
    {
        this.Dispose();

        if (Directory.Exists(this.folderPath))
        {
            Directory.Delete(this.folderPath, recursive: true);
        }
    }

    [Test]
    public void Render__WhenNothingWasChanged__ThenShouldNotOfferToSave()
    {
        // When:
        var component = this.RenderEditor();

        // Then:
        Assert.That(component.Find(SaveSelector).HasAttribute("disabled"), Is.True);
    }

    [Test]
    public void Input__WhenTheInstallFolderCannotBeReached__ThenShouldRefuseToSaveIt()
    {
        // Given:
        var component = this.RenderEditor();

        // When:
        component.Find(InstallFolderSelector).Input(UnreachableFolderPath);

        // Then:
        Assert.Multiple(() =>
        {
            Assert.That(component.FindAll(ErrorSelector), Is.Not.Empty);
            Assert.That(component.Find(SaveSelector).HasAttribute("disabled"), Is.True);
        });
    }

    [Test]
    public void Input__WhenTheInstallFolderHoldsNoRuntimeVersion__ThenShouldSaySoAndStillSaveIt()
    {
        // Given:
        this.directoryReader
            .Setup(reader => reader.ReadSubFolderNames(ReachableFolderPath))
            .Returns<string>(static _ => []);

        var component = this.RenderEditor();

        // When:
        component.Find(InstallFolderSelector).Input(ReachableFolderPath);

        // Then:
        Assert.Multiple(() =>
        {
            Assert.That(component.FindAll(WarningSelector), Is.Not.Empty);
            Assert.That(component.Find(SaveSelector).HasAttribute("disabled"), Is.False);
        });
    }

    [Test]
    public void Save__WhenTheSettingsWereWritten__ThenShouldSaySoUntilTheValueChangesAgain()
    {
        // Given:
        var component = this.RenderEditor();
        component.Find(InstallFolderSelector).Input(ReachableFolderPath);

        // When:
        component.Find(SaveSelector).Click();

        // Then:
        component.WaitForAssertion(
            () => Assert.That(component.FindAll(".settings-saved"), Is.Not.Empty));

        // When:
        // A message about a value the tester has moved on from would be describing nothing.
        component.Find(InstallFolderSelector).Input(@"D:\Ide2");

        // Then:
        Assert.That(component.FindAll(".settings-saved"), Is.Empty);
    }

    [Test]
    public void Save__WhenTheSettingsCouldNotBeWritten__ThenShouldLeaveTheTesterAbleToTryAgain()
    {
        // Given:
        // The settings file cannot be created below a file, so the write is bound to fail.
        Directory.CreateDirectory(this.folderPath);
        File.WriteAllText(Path.Combine(this.folderPath, "blocking-file"), string.Empty);

        this.settingsFilePath = Path.Combine(this.folderPath, "blocking-file", "settings.json");

        var component = this.RenderEditor();
        component.Find(InstallFolderSelector).Input(ReachableFolderPath);

        // When:
        component.Find(SaveSelector).Click();

        // Then:
        component.WaitForAssertion(
            () => Assert.That(component.FindAll(".settings-save-failed"), Is.Not.Empty));

        Assert.Multiple(() =>
        {
            Assert.That(
                component.Find(InstallFolderSelector).GetAttribute("value"),
                Is.EqualTo(ReachableFolderPath));
            Assert.That(component.Find(SaveSelector).HasAttribute("disabled"), Is.False);
        });
    }

    private AppSettingsStore CreateStore(string filePath)
    {
        var paths = new Mock<IAppPathsProvider>();
        paths.SetupGet(provider => provider.Files).Returns(new AppFilePaths(filePath));

        return new AppSettingsStore(paths.Object, new Mock<IAppLogger>().Object);
    }

    private IRenderedComponent<SettingsEditorComponent> RenderEditor()
        => this.RenderComponent<SettingsEditorComponent>();
}