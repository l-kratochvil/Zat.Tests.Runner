namespace Zat.Tests.Runner.Common.Net.Application;

using System.Diagnostics;
using Microsoft.Extensions.Hosting;

/// <summary>
/// Configuration conventions shared by the applications.
/// </summary>
public static class AppConfiguration
{
    /// <summary>
    /// The prefix of the environment variables the applications read their configuration from,
    /// e.g. <c>ZatTestsRunner_SmtpAuth__Password</c> for the key <c>SmtpAuth:Password</c>.
    /// </summary>
    public const string EnvironmentVariablesPrefix = "ZatTestsRunner_";

    /// <summary>
    /// Defaults the host environment to <see cref="Environments.Development"/> in <c>DEBUG</c> builds
    /// when neither <c>DOTNET_ENVIRONMENT</c> nor <c>ASPNETCORE_ENVIRONMENT</c> is set.
    /// </summary>
    [Conditional("DEBUG")]
    public static void UseDevelopmentWhenDebugConfig()
    {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT"))
            && string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")))
        {
            Environment.SetEnvironmentVariable(variable: "DOTNET_ENVIRONMENT", value: Environments.Development);
        }
    }
}