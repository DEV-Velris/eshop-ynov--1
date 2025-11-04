using Ordering.Application.Features.Orders.Dtos;

namespace Ordering.Application.Features.Orders.Queries.GetOrdersByCustomerId;

/// <summary>
/// Represents the result of querying orders for a specific customer.
/// </summary>
/// <param name="Orders">The collection of orders associated with the customer.</param>
public record GetOrdersByCustomerIdQueryResult(IEnumerable<OrderDto> Orders);