namespace Synaptix.Analyzers.Samples.Domain;

public sealed class Invoice
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string Number { get; set; } = "";
    public decimal Amount { get; set; }
    public DateTimeOffset IssuedAt { get; set; }
    public DateTimeOffset DueAt { get; set; }
    public InvoiceStatus Status { get; set; }
}

public enum InvoiceStatus
{
    Draft,
    Sent,
    Paid,
    Overdue,
    Cancelled,
}
