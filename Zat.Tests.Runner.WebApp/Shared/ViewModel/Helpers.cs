namespace Zat.Tests.Runner.WebApp.Shared.ViewModel;

using System.Linq.Expressions;
using System.Reflection;

/// <summary>
/// Names a property by an expression rather than by a string, so the name follows a rename.
/// </summary>
internal static class Helpers
{
    /// <summary>
    /// Gets the name of the property <paramref name="property"/> reads.
    /// </summary>
    /// <typeparam name="TOwner">The type the property belongs to.</typeparam>
    /// <typeparam name="TValue">The type the property is read as.</typeparam>
    /// <param name="property">An expression reading one property of its parameter.</param>
    /// <returns>The name of the property.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="property"/> does not read a property of <typeparamref name="TOwner"/>.
    /// </exception>
    public static string GetExpressionPropertyName<TOwner, TValue>(Expression<Func<TOwner, TValue>> property)
    {
        // A value type read as an object comes boxed, which is no part of naming it.
        var body = property.Body is UnaryExpression { NodeType: ExpressionType.Convert } converted
            ? converted.Operand
            : property.Body;

        return body is MemberExpression { Member: PropertyInfo info, Expression: ParameterExpression }
            ? info.Name
            : throw new ArgumentException(
                $"Expression '{property}' must name a property of {typeof(TOwner).Name}",
                nameof(property));
    }
}