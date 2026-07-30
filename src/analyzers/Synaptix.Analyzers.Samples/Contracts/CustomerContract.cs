namespace Synaptix.Analyzers.Samples.Contracts;

public sealed record CustomerSummary(
    Guid Id,
    string Name,
    string Email,
    DateTimeOffset CreatedAt);

public sealed record OrderSummary(
    Guid Id,
    string Reference,
    decimal Total,
    DateTimeOffset PlacedAt);

public sealed record CreateCustomerRequest(
    string Name,
    string Email);

public sealed record UpdateCustomerRequest(
    Guid Id,
    string Name,
    string Email);
