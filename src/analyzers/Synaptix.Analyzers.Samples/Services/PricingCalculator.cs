using Synaptix.Analyzers.Samples.Common;
using Synaptix.Analyzers.Samples.Domain;

namespace Synaptix.Analyzers.Samples.Services;

public sealed class PricingCalculator
{
    private readonly ILogger<PricingCalculator> _logger;

    public PricingCalculator(ILogger<PricingCalculator> logger)
    {
        _logger = logger;
    }

    public decimal CalculateTotal(IReadOnlyList<LineItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        var subtotal = items.Sum(i => i.UnitPrice * i.Quantity);
        var discount = CalculateDiscount(subtotal, items.Count);
        var tax = CalculateTax(subtotal - discount);

        var total = subtotal - discount + tax;
        _logger.LogDebug(
            "Computed total {Total} for {ItemCount} items (subtotal {Subtotal}, discount {Discount}, tax {Tax})",
            total, items.Count, subtotal, discount, tax);

        return total;
    }

    private static decimal CalculateDiscount(decimal subtotal, int itemCount)
    {
        if (itemCount >= 100)
        {
            return subtotal * 0.15m;
        }

        if (itemCount >= 10)
        {
            return subtotal * 0.05m;
        }

        return 0m;
    }

    private static decimal CalculateTax(decimal taxableAmount)
    {
        return Math.Round(taxableAmount * 0.20m, 2, MidpointRounding.AwayFromZero);
    }

    private string FormatCurrency(decimal amount)
    {
        return $"${amount:F2}";
    }
}

public sealed record LineItem(string Sku, int Quantity, decimal UnitPrice);
