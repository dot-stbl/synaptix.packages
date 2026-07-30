using System;

namespace Synaptix.Text.Case;

/// <summary>
/// Extensions converter class to more cases
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Convert <paramref name="source"/> to <c>DotCase</c>
    /// </summary>
    /// <example>example.word</example>
    /// <param name="source">compute string</param>
    /// <exception cref="ArgumentNullException">if null</exception>
    public static string ToDotCase(this string source)
    {
        return source is null
            ? throw new ArgumentNullException(nameof(source))
            : StringCaseHelpers.SymbolsPipe(
            source,
            mainDelimiter: '.',
            newWordSymbolHandler: (s, disableFrontDelimiter)
                => disableFrontDelimiter ? [char.ToLowerInvariant(s)] : ['.', char.ToLowerInvariant(s)]);
    }

    /// <summary>
    /// Convert <paramref name="source"/> to <c>CamelCase</c>
    /// </summary>
    /// <example>exampleWord</example>
    /// <param name="source">compute string</param>
    /// <exception cref="ArgumentNullException">if null</exception>
    public static string ToCamelCase(this string source)
    {
        return source is null
            ? throw new ArgumentNullException(nameof(source))
            : StringCaseHelpers.SymbolsPipe(
            source,
            mainDelimiter: '\0',
            newWordSymbolHandler: (s, disableFrontDelimiter)
                => disableFrontDelimiter ? [char.ToLowerInvariant(s)] : [char.ToUpperInvariant(s)]);
    }

    /// <summary>
    /// Convert <paramref name="source"/> to <c>KebabCase</c>
    /// </summary>
    /// <example>example-word</example>
    /// <param name="source">compute string</param>
    /// <exception cref="ArgumentNullException">if null</exception>
    public static string ToKebabCase(this string source)
    {
        return source is null
            ? throw new ArgumentNullException(nameof(source))
            : StringCaseHelpers.SymbolsPipe(
            source,
            mainDelimiter: '-',
            newWordSymbolHandler: (s, disableFrontDelimiter)
                => disableFrontDelimiter ? [char.ToLowerInvariant(s)] : ['-', char.ToLowerInvariant(s)]);
    }

    /// <summary>
    /// Convert <paramref name="source"/> to <c>SnakeCase</c>
    /// </summary>
    /// <example>example_word</example>
    /// <param name="source">compute string</param>
    /// <exception cref="ArgumentNullException">if null</exception>
    public static string ToSnakeCase(this string source)
    {
        return source is null
            ? throw new ArgumentNullException(nameof(source))
            : StringCaseHelpers.SymbolsPipe(
            source,
            mainDelimiter: '_',
            newWordSymbolHandler: (s, disableFrontDelimiter)
                => disableFrontDelimiter ? [char.ToLowerInvariant(s)] : ['_', char.ToLowerInvariant(s)]);
    }

    /// <summary>
    /// Convert <paramref name="source"/> to <c>PascalCase</c>
    /// </summary>
    /// <example>ExampleWord</example>
    /// <param name="source">compute string</param>
    /// <exception cref="ArgumentNullException">if null</exception>
    public static string ToPascalCase(this string source)
    {
        return source is null
            ? throw new ArgumentNullException(nameof(source))
            : StringCaseHelpers.SymbolsPipe(
            source,
            mainDelimiter: '\0',
            newWordSymbolHandler: (s, _) => [char.ToUpperInvariant(s)]);
    }

    /// <summary>
    /// Convert <paramref name="source"/> to <c>TrainCase</c>
    /// </summary>
    /// <example>Example-Word</example>
    /// <param name="source">compute string</param>
    /// <exception cref="ArgumentNullException">if null</exception>
    public static string ToTrainCase(this string source)
    {
        return source is null
            ? throw new ArgumentNullException(nameof(source))
            : StringCaseHelpers.SymbolsPipe(
            source,
            mainDelimiter: '-',
            newWordSymbolHandler: (s, disableFrontDelimiter)
                => disableFrontDelimiter ? [char.ToUpperInvariant(s)] : ['-', char.ToUpperInvariant(s)]);
    }
}