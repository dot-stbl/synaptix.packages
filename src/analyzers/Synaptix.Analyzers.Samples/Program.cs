using Synaptix.Analyzers.Samples.Common;
using Synaptix.Analyzers.Samples.Legacy;
using Synaptix.Analyzers.Samples.Services;

namespace Synaptix.Analyzers.Samples;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var logger = new ConsoleLogger();

        var connector = new DatabaseConnector("Server=localhost;Database=samples;", logger);
        await connector.OpenAsync();

        var pricing = new PricingCalculator(new TypedConsoleLogger<PricingCalculator>());
        var items = new List<LineItem>
        {
            new("SKU-001", 3, 19.99m),
            new("SKU-002", 1, 49.50m),
        };
        var total = pricing.CalculateTotal(items);

        Console.WriteLine($"Total: {total}");
    }
}
