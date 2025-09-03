using Synaptix.Color.Models.Storages.Interfaces;
using Synaptix.Color.Typed;

namespace Synaptix.Color.Models.Storages;

/// <summary>
/// <see cref="Rgb"/> internal color info storage
/// </summary>
public record RgbByteStorage : IByteStorage
{
    /// <summary>
    /// <c>Red</c> color byte
    /// </summary>
    public byte R { get; set; }

    /// <summary>
    /// <c>Green</c> color byte
    /// </summary>
    public byte G { get; set; }

    /// <summary>
    /// <c>Blue</c> color byte
    /// </summary>
    public byte B { get; set; }

    /// <summary>
    /// Create default zero-zero-zero rgb byte storage
    /// </summary>
    public static RgbByteStorage Default => new() { R = 0, G = 0, B = 0 };
}