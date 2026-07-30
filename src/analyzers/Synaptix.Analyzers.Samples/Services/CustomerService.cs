using Synaptix.Analyzers.Samples.Common;
using Synaptix.Analyzers.Samples.Contracts;
using Synaptix.Analyzers.Samples.Domain;

namespace Synaptix.Analyzers.Samples.Services;

public sealed class CustomerService
{
    private readonly ICustomerRepository _repository;
    private readonly ILogger<CustomerService> _logger;
    private readonly TimeProvider _clock;

    public CustomerService(
        ICustomerRepository repository,
        ILogger<CustomerService> logger,
        TimeProvider clock)
    {
        _repository = repository;
        _logger = logger;
        _clock = clock;
    }

    public async Task<Customer> CreateAsync(
        CreateCustomerRequest request,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        ArgumentException.ThrowIfNullOrEmpty(request.Name);
        ArgumentException.ThrowIfNullOrEmpty(request.Email);

        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            CreatedAt = _clock.GetUtcNow(),
        };

        await _repository.AddAsync(customer, ct);

        _logger.LogInformation(
            "Created customer {CustomerId} with email {Email}",
            customer.Id, customer.Email);

        return customer;
    }

    public async Task<Customer?> FindByEmailAsync(
        string email,
        CancellationToken ct = default)
    {
        return await _repository.FindByEmailAsync(email.ToLowerInvariant(), ct);
    }

    public async Task<CustomerSummary> GetSummaryAsync(
        Guid id,
        CancellationToken ct = default)
    {
        var customer = await _repository.GetByIdAsync(id, ct)
            ?? throw new InvalidOperationException($"Customer {id} not found");

        return new CustomerSummary(
            customer.Id,
            customer.Name,
            customer.Email,
            customer.CreatedAt);
    }

    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        var at = email.IndexOf('@');
        return at > 0 && at < email.Length - 1;
    }

    private string FormatDisplayName(Customer customer)
    {
        return customer.DisplayName ?? customer.Name;
    }
}

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Customer?> FindByEmailAsync(string email, CancellationToken cancellationToken);
    Task AddAsync(Customer customer, CancellationToken cancellationToken);
    Task UpdateAsync(Customer customer, CancellationToken cancellationToken);
}
