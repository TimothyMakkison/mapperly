using System.Runtime.CompilerServices;

namespace Riok.Mapperly.Abstractions;

/// <summary>
/// Specifies options for a property mapping.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class MapPropertyAttribute : Attribute
{
    private const string PropertyAccessSeparatorStr = ".";
    private const char PropertyAccessSeparator = '.';
    private const string NameOfFunction = "nameof(";

    /// <summary>
    /// Maps a specified source property to the specified target property.
    /// </summary>
    /// <param name="source">The name of the source property. The use of `nameof()` is encouraged. A path can be specified by joining property names with a '.'.</param>
    /// <param name="target">The name of the target property. The use of `nameof()` is encouraged. A path can be specified by joining property names with a '.'.</param>
    /// <param name="internalSource">Used internally, do not change.</param>
    /// <param name="internalTarget">Used internally, do not change.</param>
    public MapPropertyAttribute(
        string source,
        string target,
        [CallerArgumentExpression(nameof(source))] string internalSource = default!,
        [CallerArgumentExpression(nameof(target))] string internalTarget = default!
    )
        : this(
            ChooseProperty(source, internalSource).Split(PropertyAccessSeparator),
            ChooseProperty(target, internalTarget).Split(PropertyAccessSeparator)
        ) { }

    private static string ChooseProperty(string value, string? expression)
    {
        if (expression is null)
            return value;

        var start = expression.IndexOf(NameOfFunction);
        if (start is -1 or > 0)
            return value;
        var end = expression.EndsWith(")");
        if (!end)
            return value;

        var stripped = expression.Substring(7, expression.Length - 1 - NameOfFunction.Length);

        var s = string.Join(".", stripped.Split('.').Skip(1));
        return s;
    }

    /// <summary>
    /// Maps a specified source property to the specified target property.
    /// </summary>
    /// <param name="source">The path of the source property. The use of `nameof()` is encouraged.</param>
    /// <param name="target">The path of the target property. The use of `nameof()` is encouraged.</param>
    public MapPropertyAttribute(string[] source, string[] target)
    {
        Source = source;
        Target = target;
    }

    /// <summary>
    /// Gets the name of the source property.
    /// </summary>
    public IReadOnlyCollection<string> Source { get; }

    /// <summary>
    /// Gets the full name of the source property path.
    /// </summary>
    public string SourceFullName => string.Join(PropertyAccessSeparatorStr, Source);

    /// <summary>
    /// Gets the name of the target property.
    /// </summary>
    public IReadOnlyCollection<string> Target { get; }

    /// <summary>
    /// Gets the full name of the target property path.
    /// </summary>
    public string TargetFullName => string.Join(PropertyAccessSeparatorStr, Target);
}
