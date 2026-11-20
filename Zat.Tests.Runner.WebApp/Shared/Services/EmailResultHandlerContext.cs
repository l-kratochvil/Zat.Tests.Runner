namespace Zat.Tests.Runner.WebApp.Shared.Services;

using Fluxor;
using Zat.Tests.Runner.Common.Net.Services;
using Zat.Tests.Runner.WebApp.Shared.Stores.TestConfiguration;

internal class EmailResultHandlerContext(
    IState<TestConfigurationState> testConfigurationState)
    : EmailResultHandler.IContext
{
    /// <inheritdoc/>
    public bool IsEmailNotificationEnabled
        => testConfigurationState.Value.IsEmailNotificationEnabled;

    /// <inheritdoc/>
    public IReadOnlyList<string> EmailAddresses
        => testConfigurationState.Value.EmailAddresses;
}
