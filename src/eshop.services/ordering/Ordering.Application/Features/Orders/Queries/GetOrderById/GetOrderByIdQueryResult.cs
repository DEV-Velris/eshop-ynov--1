using Ordering.Application.Features.Orders.Dtos;

namespace Ordering.Application.Features.Orders.Queries.GetOrderById;

/// <summary>
/// Represents the result of querying an order by its ID.
/// </summary>
/// <param name="Order">The order that matches the specified ID.</param>
public record GetOrderByIdQueryResult(OrderDto Order);