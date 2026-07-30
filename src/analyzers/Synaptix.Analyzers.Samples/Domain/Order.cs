namespace Synaptix.Analyzers.Samples.Domain;

public sealed class Order
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public required string Reference { get; set; }
    public decimal Total { get; set; }
    public DateTimeOffset PlacedAt { get; set; }
}
