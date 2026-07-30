using Synaptix.Analyzers.Samples.Common;
using Synaptix.Analyzers.Samples.Domain;

namespace Synaptix.Analyzers.Samples.Services;

public sealed class InvoiceAggregator(
    IInvoiceRepository repository,
    ILogger<InvoiceAggregator> logger,
    TimeProvider clock)
{
    public async Task<InvoiceSummary> AggregateAsync(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        var invoices = await repository.GetByOrderAsync(orderId, cancellationToken);
        if (invoices.Count == 0)
        {
            return new InvoiceSummary(orderId, 0m, 0m, 0m, []);
        }

        var total = 0m;
        var paid = 0m;
        var outstanding = 0m;
        var entries = new List<InvoiceEntry>(invoices.Count);

        foreach (var invoice in invoices)
        {
            total += invoice.Amount;

            if (invoice.Status == InvoiceStatus.Paid)
            {
                paid += invoice.Amount;
            }
            else
            {
                outstanding += invoice.Amount;
            }

            entries.Add(new InvoiceEntry(
                invoice.Id,
                invoice.Number,
                invoice.Amount,
                invoice.Status,
                invoice.DueAt));
        }

        logger.LogInformation(
            "Aggregated {Count} invoices for order {OrderId}",
            entries.Count, orderId);

        return new InvoiceSummary(orderId, total, paid, outstanding, entries);
    }

    public async Task NotifyOverdueAsync(
        CancellationToken cancellationToken = default)
    {
        var cutoff = clock.GetUtcNow();
        var overdue = await repository.ListOverdueAsync(cutoff, cancellationToken);

        logger.LogInformation(
            "Found {Count} overdue invoices as of {Cutoff}",
            overdue.Count, cutoff);

        foreach (var invoice in overdue)
        {
            _ = await repository.MarkOverdueAsyncAsync(invoice.Id, cancellationToken);
        }
    }
}

public sealed record InvoiceSummary(
    Guid OrderId,
    decimal Total,
    decimal Paid,
    decimal Outstanding,
    IReadOnlyList<InvoiceEntry> Entries);

public sealed record InvoiceEntry(
    Guid Id,
    string Number,
    decimal Amount,
    InvoiceStatus Status,
    DateTimeOffset DueAt);

public interface IInvoiceRepository
{
    Task<IReadOnlyList<Invoice>> GetByOrderAsync(Guid orderId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Invoice>> ListOverdueAsync(DateTimeOffset cutoff, CancellationToken cancellationToken);
    Task<int> MarkOverdueAsyncAsync(Guid invoiceId, CancellationToken cancellationToken);
}
