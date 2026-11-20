namespace Zat.Tests.Runner.TuiApp.Screens;

using System.Threading.Tasks;

internal interface IScreen
{
    public Task<RenderOutput> RenderAsync();
}