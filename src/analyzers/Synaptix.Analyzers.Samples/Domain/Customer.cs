namespace Synaptix.Analyzers.Samples.Domain;

public sealed class Customer
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string? DisplayName { get; set; }
}
