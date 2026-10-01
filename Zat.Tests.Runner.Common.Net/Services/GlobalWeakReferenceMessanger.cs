namespace Zat.Tests.Runner.Common.Net.Services;

/// <summary>
/// A global messenger service. It's purpose is to distinguish global messanger service form the scoped messanger service.
/// </summary>
public class GlobalWeakReferenceMessanger : Messanger;