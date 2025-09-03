using Synaptix.Color.Models.Interfaces;
using Synaptix.Color.Models.Storages;

namespace Synaptix.Color.Typed;

/// <summary>
/// <c>RGB</c> base color type
/// </summary>
public class Rgb : IColor<RgbByteStorage>
{
    /// <inheritdoc />
    public RgbByteStorage Value { get; private set; } = RgbByteStorage.Default;
    
    /// <summary>
    /// Create <see cref="Rgb"/> by params byte
    /// </summary>
    public static Rgb TryCreate(byte r, byte g, byte b)
    {
        return new Rgb
        {
            Value = new RgbByteStorage
            {
                R = r,
                G = g,
                B = b
            }
        };
    }
}