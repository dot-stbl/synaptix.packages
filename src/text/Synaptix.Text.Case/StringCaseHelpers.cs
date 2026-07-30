using System;
using System.Linq;
using System.Text;

namespace Synaptix.Text.Case;

/// <summary>
/// Internal pipeline that walks the source string character-by-character
/// and pipes each symbol through the new-word symbol handler.
/// </summary>
internal static class StringCaseHelpers
{
    internal static readonly char[] Delimiters = [' ', '-', '_', '.'];

    /// <summary>
    /// Walk <paramref name="source"/>, splitting on <see cref="Delimiters"/>,
    /// emitting delimiters as <paramref name="mainDelimiter"/> between words
    /// (kept as-is when seen) and otherwise delegating character shaping to
    /// <paramref name="newWordSymbolHandler"/>.
    /// </summary>
    internal static string SymbolsPipe(
        string source,
        char mainDelimiter,
        Func<char, bool, char[]> newWordSymbolHandler)
    {
        var builder = new StringBuilder();

        var disableFrontDelimiter = true;
        var nextSymbolStartsNewWord = true;

        foreach (var symbol in source)
        {
            if (Delimiters.Contains(symbol))
            {
                if (symbol == mainDelimiter)
                {
                    builder.Append(symbol);
                    disableFrontDelimiter = true;
                }

                nextSymbolStartsNewWord = true;
            }
            else if (!char.IsLetterOrDigit(symbol))
            {
                builder.Append(symbol);
                disableFrontDelimiter = true;
                nextSymbolStartsNewWord = true;
            }
            else
            {
                if (nextSymbolStartsNewWord || char.IsUpper(symbol))
                {
                    builder.Append(newWordSymbolHandler(symbol, disableFrontDelimiter));
                    disableFrontDelimiter = false;
                    nextSymbolStartsNewWord = false;
                }
                else
                {
                    builder.Append(symbol);
                }
            }
        }

        return builder.ToString();
    }
}