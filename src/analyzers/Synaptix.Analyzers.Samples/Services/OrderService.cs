using Synaptix.Analyzers.Samples.Common;
using Synaptix.Analyzers.Samples.Contracts;
using Synaptix.Analyzers.Samples.Domain;

namespace Synaptix.Analyzers.Samples.Services;

public sealed class OrderService(
    IOrderRepository repository,
    ILogger<OrderService> logger,
    TimeProvider clock)
{
    public async Task<Order> PlaceAsync(
        PlaceOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = request.CustomerId,
            Reference = request.Reference,
            Total = request.Total,
            PlacedAt = clock.GetUtcNow(),
        };

        await repository.SaveAsync(order, cancellationToken);

        logger.LogInformation(
            "Placed order {OrderId} for customer {CustomerId} with total {Total}",
            order.Id, order.CustomerId, order.Total);

        return order;
    }

    public async Task<OrderSummary?> GetSummaryAsync(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        var order = await repository.GetByIdAsync(orderId, cancellationToken);
        if (order is null)
        {
            return null;
        }

        return new OrderSummary(
            order.Id,
            order.Reference,
            order.Total,
            order.PlacedAt);
    }

    public async Task<IReadOnlyList<OrderSummary>> ListForCustomerAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        var orders = await repository.ListByCustomerAsync(customerId, cancellationToken);
        return orders
            .Select(o => new OrderSummary(o.Id, o.Reference, o.Total, o.PlacedAt))
            .ToList();
    }
}

public sealed record PlaceOrderRequest(
    Guid CustomerId,
    string Reference,
    decimal Total);

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Order>> ListByCustomerAsync(Guid customerId, CancellationToken cancellationToken);
    Task SaveAsync(Order order, CancellationToken cancellationToken);
}
