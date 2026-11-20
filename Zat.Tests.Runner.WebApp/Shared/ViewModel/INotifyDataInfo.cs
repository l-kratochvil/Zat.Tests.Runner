namespace Zat.Tests.Runner.WebApp.Shared.ViewModel;

/// <summary>
/// Desrcibes an object that notfies about data changes.
/// </summary>
public interface INotifyDataInfo
{
    public event EventHandler? DataChanged;
}