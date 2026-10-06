namespace Zat.Tests.Runner.TuiApp;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Zat.Tests.Runner.Common.Net.Services;
using Zat.Tests.Runner.TuiApp.Screens;

internal class App
{
    public static async Task RunAsync(IHost host)
    {
        try
        {
            // A test tree that can't be read at start ends the application, which is the only way the TUI tells why
            // there would be nothing to choose from. Later changes of the test assembly are followed on their own.
            await host.Services.GetRequiredService<ITestTreeStore>().DiscoverAsync();

            using var scope = host.Services.CreateScope();
            await MainRenderAsync(scope.ServiceProvider.GetRequiredService<HomeScreen>());
        }
        catch (Exception ex)
        {
            Clear();

            WriteLine("Při běhu aplikace Zat.Tests.Runner se vyskytla chyba:"); // TODO: Localize text
            WriteException(ex);
#if DEBUG
            if (Debugger.IsAttached)
            {
                throw;
            }
#endif

            WriteLine("Aplikaci ukončíte libovolnou klávesou..."); // TODO: Localize text

            AnsiConsole.Console.Input.ReadKey(true);
        }
    }

    public static async Task MainRenderAsync(IScreen initScreen)
    {
        var currentScreen = initScreen;

        var screens = new Stack<IScreen>();

        while (true)
        {
            currentScreen ??= screens.TryPop(out var nextScreen)
                ? nextScreen // Redirect one screen back if not next screen not provided
                : initScreen; // Or return to the initial screen

            var renderOutput = await currentScreen.RenderAsync();
            if (renderOutput.Exit)
            {
                break;
            }

            if (renderOutput.NextScreen is not null)
            {
                screens.Push(currentScreen);
            }

            currentScreen = renderOutput.NextScreen;
        }
    }
}