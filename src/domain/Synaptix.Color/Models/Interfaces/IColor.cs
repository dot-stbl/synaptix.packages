namespace Synaptix.Color.Models.Interfaces;

/// <summary>
/// Base <c>color</c> interface
/// </summary>
public interface IColor<out TValue> where TValue : class
{
    /// <summary>
    /// Internal typed color value
    /// </summary>
    public TValue Value { get; }
}