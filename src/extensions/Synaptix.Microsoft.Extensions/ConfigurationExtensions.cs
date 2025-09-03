using System.Reflection;
using Microsoft.Extensions.Configuration;
using Synaptix.Microsoft.Extensions.Attributes;

namespace Synaptix.Microsoft.Extensions;

/// <summary>
/// Provides extension methods for IConfiguration to simplify retrieval of external options with section override support.
/// This class enables convenient access to configuration sections that may be overridden by custom attributes,
/// making it easier to work with external configuration sources and custom section mappings.
/// </summary>
public static class ConfigurationExtensions
{
    /// <summary>
    /// Gets the external options of the specified type from configuration, if a SectionOverrideAttribute is present.
    /// This method attempts to retrieve configuration values using the path specified in the SectionOverrideAttribute
    /// and maps them to the target type. Returns null if no attribute is found or section is not available.
    /// </summary>
    /// <typeparam name="TExternalOptions">The type of external options to retrieve</typeparam>
    /// <param name="configuration">The configuration instance to read from</param>
    /// <returns>The configured options object, or null if no SectionOverrideAttribute is found or section is unavailable</returns>
    public static TExternalOptions? GetExternalOptions<TExternalOptions>(this IConfiguration configuration)
        where TExternalOptions : class
    {
        return typeof(TExternalOptions).GetCustomAttribute<SectionOverrideAttribute>() is { Path: { } path } &&
               configuration.GetSection(path) is { } section
            ? section.Get<TExternalOptions>()
            : null;
    }
    
    /// <summary>
    /// Gets the required external options of the specified type from configuration.
    /// This method ensures that a SectionOverrideAttribute exists and the corresponding configuration section is available,
    /// throwing exceptions if either requirement is not met. The method throws an exception if the section cannot be found
    /// or if mapping to the target type fails.
    /// </summary>
    /// <typeparam name="TExternalOptions">The type of external options to retrieve</typeparam>
    /// <param name="configuration">The configuration instance to read from</param>
    /// <returns>The configured options object, guaranteed to be non-null</returns>
    /// <exception cref="InvalidOperationException">Thrown when no SectionOverrideAttribute is found or section is not available</exception>
    public static TExternalOptions GetRequiredExternalOptions<TExternalOptions>(this IConfiguration configuration)
        where TExternalOptions : class
    {
        if (typeof(TExternalOptions).GetCustomAttribute<SectionOverrideAttribute>() is not { Path: { } path })
        {
            throw new InvalidOperationException($"Not found attribute {nameof(SectionOverrideAttribute)}");
        }

        if (configuration.GetSection(path) is not { } section)
        {
            throw new InvalidOperationException($"Not found section by {path}");
        }

        return section.Get<TExternalOptions>() ??
               throw new InvalidOperationException($"Wrong get model by section '{section}'");
    }
    
    /// <summary>
    /// Gets the override configuration section for the specified external options type.
    /// This internal method retrieves the configuration section path specified in the SectionOverrideAttribute
    /// and returns the corresponding IConfigurationSection if found.
    /// </summary>
    /// <typeparam name="TExternalOptions">The type of external options to get override section for</typeparam>
    /// <param name="configuration">The configuration instance to search in</param>
    /// <returns>The configuration section if found, or null if no SectionOverrideAttribute is present</returns>
    internal static IConfigurationSection? GetOverrideSection<TExternalOptions>(this IConfiguration configuration)
    {
        if (typeof(TExternalOptions).GetCustomAttribute<SectionOverrideAttribute>() is { } customAttribute)
        {
            return configuration.GetSection(customAttribute.Path);
        }

        return null;
    }
}