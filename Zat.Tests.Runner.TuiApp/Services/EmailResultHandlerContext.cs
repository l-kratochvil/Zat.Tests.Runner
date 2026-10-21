namespace Zat.Tests.Runner.TuiApp.Services;

using Zat.Tests.Runner.Common.Net.Services;
using Zat.Tests.Runner.TuiApp.Stores;

internal class EmailResultHandlerContext(
    TestRunConfigStore testRunConfigStore)
    : EmailResultHandler.IContext
{
    /// <inheritdoc/>
    public bool IsEmailNotificationEnabled
        => testRunConfigStore.IsEmailNotificationEnabled ?? true;

    /// <inheritdoc/>
    public IReadOnlyList<string> EmailAddresses
        => testRunConfigStore.EmailAddresses;
}