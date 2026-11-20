namespace Zat.Tests.Runner.WebApp.Tests.Shared.Validation;

using System.Linq;

using Zat.Tests.Runner.WebApp.Shared.Validation;

/// <summary>
/// What a failed assertion says about the <see cref="Validity"/> it did not expect.
/// </summary>
/// <remarks>
/// Lives with the tests rather than with <see cref="Validity"/> itself: nothing the tester looks at
/// shows the problems as one line, so an application that offered it would invite a screen to be
/// built that way.
/// </remarks>
internal static class ValidityExtensions
{
    extension(Validity validity)
    {
        /// <summary>
        /// Gets all problem messages as one line, so a failed assertion names what was wrong.
        /// </summary>
        public string Summary
            => string.Join(" ", validity.Issues.Select(static issue => issue.Message));
    }
}
