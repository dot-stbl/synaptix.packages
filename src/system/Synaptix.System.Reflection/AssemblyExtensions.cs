using System.Reflection;

namespace Synaptix.System.Reflection;

/// <summary>
/// Provides extension methods for working with Assembly objects and their referenced types.
/// This class contains utility methods for discovering and collecting all types from an assembly and its dependencies.
/// </summary>
public static class AssemblyExtensions
{
    /// <summary>
    /// Gets all unique types from the specified assembly and its referenced assemblies.
    /// This method recursively loads referenced assemblies and collects all available types,
    /// ensuring no duplicate types are returned in the result.
    /// </summary>
    /// <param name="assembly">The assembly to get types from, or null if no assembly is provided</param>
    /// <returns>A read-only collection of unique types from the assembly and its referenced assemblies</returns>
    public static IReadOnlyCollection<Type> GetReferencedTypes(this Assembly? assembly)
    {
        return assembly?.LoadReferencedAssemblies()
            .SelectMany(currentAssembly => currentAssembly.GetTypes())
            .Concat(assembly.GetTypes())
            .Distinct()
            .ToArray() ?? [];
    }

    /// <summary>
    /// Attempts to get all referenced assemblies from the specified assembly.
    /// This method loads each referenced assembly and returns them as a collection.
    /// </summary>
    /// <param name="assembly">The assembly to get referenced assemblies from, or null if no assembly is provided</param>
    /// <returns>A read-only collection of referenced assemblies, or empty collection if input is null</returns>
    public static IReadOnlyCollection<Assembly> LoadReferencedAssemblies(this Assembly? assembly)
    {
        if (assembly == null)
        {
            return [];
        }

        return assembly.GetReferencedAssemblies()
            .Select(Assembly.Load)
            .ToArray();
    }
}