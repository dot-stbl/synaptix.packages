using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Synaptix.Microsoft.Extensions;

/// <summary>
/// Provides extension methods for configuring external options with SectionOverrideAttribute support in dependency injection containers.
/// This class enables seamless integration between Microsoft.Extensions.Configuration and Microsoft.Extensions.DependencyInjection,
/// allowing for flexible configuration binding of external options classes that specify custom section paths via attributes.
/// </summary>
public static class OptionsExtensions
{
    /// <summary>
    /// Adds external options of the specified type to the service collection using configuration binding.
    /// This method retrieves the configuration section path from the SectionOverrideAttribute applied to the options type,
    /// binds the configuration section to the options type, and registers it with the dependency injection container.
    /// Throws an exception if no SectionOverrideAttribute is found or the corresponding configuration section is not available.
    /// </summary>
    /// <typeparam name="TExternalOptions">The type of external options to configure</typeparam>
    /// <param name="serviceCollection">The service collection to register the options with</param>
    /// <param name="configuration">The configuration instance containing the section data</param>
    /// <returns>The service collection for method chaining</returns>
    /// <exception cref="InvalidOperationException">Thrown when no SectionOverrideAttribute is found or section is not available</exception>
    public static IServiceCollection AddOptionsExternal<TExternalOptions>(
        this IServiceCollection serviceCollection,
        IConfiguration configuration) where TExternalOptions : class
    {
        if (configuration.GetOverrideSection<TExternalOptions>() is { } overrideSection)
        {
            serviceCollection
                .AddOptions<TExternalOptions>()
                .Bind(overrideSection);
        }
        else
        {
            throw new InvalidOperationException("Wrong get section by OverrideAttribute");
        }

        return serviceCollection;
    }
}