namespace Synaptix.Microsoft.Extensions.Attributes;

/// <summary>
/// Represents an attribute that specifies the configuration section path for external options classes.
/// This attribute enables mapping between configuration sections and strongly-typed options objects,
/// allowing for flexible configuration management with custom section paths.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class SectionOverrideAttribute(string path) : Attribute
{
    /// <summary>
    /// Gets the path to the configuration section that should be used for this options class.
    /// This property stores the section path specified when the attribute is applied to a class.
    /// </summary>
    internal string Path { get; set; } = path;
}