namespace Zat.Tests.Runner.TuiApp.Screens;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using DevKit.Core.Extensions;

using Spectre.Console.Rendering;

using WindowsInput;
using WindowsInput.Native;

using Zat.Tests.Runner.TuiApp.Common;

internal abstract class ScreenBase : IScreen
{
    private readonly Lazy<ICommand[]> lazyCommands;
    private readonly Lazy<ScreenRenderer> lazyRenderer;

    private readonly InputSimulator inputSimulator = new();

    protected ScreenBase(
        Lazy<HomeScreen> homeScreen,
        Lazy<ExitScreen> exitScreen,
        Lazy<SettingsScreen> settingsScreen)
    {
        this.HomeScreenLazy = homeScreen;
        this.ExitScreenLazy = exitScreen;
        this.SettingsScreenLazy = settingsScreen;

        this.lazyCommands = new Lazy<ICommand[]>(() => [..this.InitCommands()]);

        this.lazyRenderer = new Lazy<ScreenRenderer>(
            () => this.CreateRenderer().Pipe(renderer =>
            {
                renderer.Commands = this.Commands;
                return renderer;
            }));
    }

    protected virtual ICommand[] AdditionalCommands { get; } = [];

    protected Lazy<HomeScreen> HomeScreenLazy { get; }

    protected Lazy<ExitScreen> ExitScreenLazy { get; }

    protected Lazy<SettingsScreen> SettingsScreenLazy { get; }

    protected ICommand[] Commands
        => this.lazyCommands.Value;

    protected ScreenRenderer Renderer
        => this.lazyRenderer.Value;

    protected virtual Configuration Config { get; init; } = new();

    protected abstract ScreenRenderer CreateRenderer();

    /// <inheritdoc/>
    public async Task<RenderOutput> RenderAsync()
    {
        Clear();

        this.Renderer.Toolbar();
        this.Renderer.Info();

        using var renderCts = new CancellationTokenSource();
        using var keyPressedCts = new CancellationTokenSource();

        var interuptRenderByKeyTask = this.HandleCommandsAsync(renderCts, keyPressedCts.Token);
        var output = await this.Renderer.Main(renderCts.Token);

        foreach (var cts in new[] { renderCts, keyPressedCts })
        {
            await cts.CancelAsync();
        }

        var interuptRenderByKeyTaskResult = await interuptRenderByKeyTask;

        return output switch
        {
            InterruptedShowPrompt => new RenderOutput(
                NextScreen: this.Commands
                    .OfType<InterruptionCommand>()
                    .FirstOrDefault(command => command.Key == interuptRenderByKeyTaskResult)
                    .CheckIsNotNull($"Interruption command not found for key '{interuptRenderByKeyTaskResult}'")
                    .NextScreen),
            CompletedShowPrompt completedRenderOutput => completedRenderOutput.RenderOutput,
            _ => throw new NotSupportedException($"Unknown show prompt result type '{output.GetType().Name}'"),
        };
    }

    protected static async Task<ShowPromptResult> ShowLiveDataAsync<TUpdateTarget, TData>(
        TUpdateTarget updateTarget,
        TData data,
        ConsoleUtils.ShowLiveDataUpdator<TUpdateTarget, TData> liveDataUpdator,
        Func<TData, RenderOutput> onSucces,
        CancellationToken ct)
        where TUpdateTarget : IRenderable
        => await ConsoleUtils.ShowLiveDataAsync(updateTarget, data, liveDataUpdator, ct) switch
        {
            true => new CompletedShowPrompt(onSucces(data)),
            false => new InterruptedShowPrompt(),
        };

    protected static async Task<ShowPromptResult> ShowPromptAsync<T>(
        IPrompt<T> prompt,
        Func<T, RenderOutput> onSucces,
        CancellationToken ct,
        Func<T, ValidationResult>? validator = null)
        => await ConsoleUtils.ShowPromptAsync(prompt, ct, validator)
            switch
            {
                (true, { } promptResult) => new CompletedShowPrompt(onSucces(promptResult)),
                (false, _) => new InterruptedShowPrompt(),
            };

    private IEnumerable<ICommand> InitCommands()
    {
        yield return new InterruptionCommand(
            Key: VirtualKeyCode.ESCAPE,
            Text: Resources.Exit_CommandText,
            NextScreen: this.ExitScreenLazy.Value);

        if (this.Config.IsBackCommandEnabled)
        {
            yield return new InterruptionCommand(
                Key: VirtualKeyCode.F1,
                Text: Resources.Back_CommandText,
                NextScreen: null);
        }

        foreach (var command in this.AdditionalCommands)
        {
            yield return command;
        }

        if (this.Config.IsSettingsCommandEnabled)
        {
            yield return new InterruptionCommand(
                Key: VirtualKeyCode.F10,
                Text: Resources.Settings_CommandText,
                NextScreen: this.SettingsScreenLazy.Value);
        }

        if (this.Config.IsHomeCommandEnabled)
        {
            yield return new InterruptionCommand(
                Key: VirtualKeyCode.F12,
                Text: Resources.Home_CommandText,
                NextScreen: this.HomeScreenLazy.Value);
        }
    }

    /// <summary>
    /// Handles the commands and returns the key that interrupted the render.
    /// </summary>
    /// <param name="renderCts">The <see cref="CancellationTokenSource"/> for the render operation.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The pressed key that interrupted the render.</returns>
    private async Task<VirtualKeyCode> HandleCommandsAsync(CancellationTokenSource renderCts, CancellationToken ct)
    {
        VirtualKeyCode GetPressedKey() => Enum
            .GetValues<VirtualKeyCode>()
            .FirstOrDefault(this.inputSimulator.InputDeviceState.IsKeyDown);

        const int waitTimeMs = 10;

        // ReSharper disable once MethodSupportsCancellation
        return await Task.Run(async () =>
        {
            var interruptionKeys = this.Commands
                .OfType<InterruptionCommand>()
                .Select(command => command.Key)
                .ToArray();

            var actionCommands = this.Commands
                .OfType<ActionCommand>()
                .ToDictionary(x => x.Key, x => x);

            try
            {
                while (!ct.IsCancellationRequested)
                {
                    var pressedKey = GetPressedKey();
                    while (pressedKey == GetPressedKey()) // Wait for key release
                    {
                        await Task.Delay(waitTimeMs, ct);
                    }

                    if (actionCommands.TryGetValue(pressedKey, out var actionCommand))
                    {
                        actionCommand.Action();
                        continue;
                    }

                    if (interruptionKeys.Contains(pressedKey))
                    {
                        await renderCts.CancelAsync();
                        return pressedKey;
                    }

                    await Task.Delay(waitTimeMs, ct);
                }
            }
            catch (TaskCanceledException)
            {
                // Ignore
            }

            return VirtualKeyCode.NONAME;
        });
    }

    public record Configuration(
        bool IsBackCommandEnabled = true,
        bool IsHomeCommandEnabled = true,
        bool IsSettingsCommandEnabled = true);
}