namespace Zat.Tests.Runner.TuiApp.Screens;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Zat.Tests.Runner.TuiApp.Common;

internal class ScreenRenderer
{
    public delegate Task<ShowPromptResult> MainRender(CancellationToken cancellationToken);

    public delegate void StatusRender();

    /// <summary>
    /// Renderer for Main section
    /// </summary>
    public required MainRender Main { get; init; }

    /// <summary>
    /// Renderer for Info section
    /// </summary>
    public StatusRender Info { get; set; } = () => { };

    public void Toolbar()
    {
        Write(new Table().Pipe(table =>
        {
            this.Commands
                .Select(command => $"{command.Text} {$"[{command.Key}]".EscapeMarkup()}")
                .ForEach(column => table.AddColumn(column));
            return table;
        }));
    }

    public ICommand[] Commands { get; set; } = [];
}