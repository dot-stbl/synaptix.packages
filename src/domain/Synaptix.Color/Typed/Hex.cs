using Synaptix.Color.Models.Interfaces;

namespace Synaptix.Color.Typed;

/// <summary>
/// <c>Hex</c> type via <see cref="IColor{TValue}"/>
/// </summary>
public record Hex : IColor<string>
{
    /// <inheritdoc />
    public string Value { get; private set; } = string.Empty;


    /// <summary>
    /// Create <see cref="Hex"/> color type
    /// </summary>
    /// <param name="value">string color value with # or without</param>
    public static Hex TryCreate(string value)
    {
        if (value.IndexOf('#') == 1 && value.Substring(1) is { } hexed)
        {
            return new Hex
            {
                Value = hexed
            };
        }

        return new Hex
        {
            Value = value
        };
    }

    /// <inheritdoc />
    public override string ToString() => $"#{Value}";
}