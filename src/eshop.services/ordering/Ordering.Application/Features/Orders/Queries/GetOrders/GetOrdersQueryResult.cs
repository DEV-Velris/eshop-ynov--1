using Ordering.Domain.Models;

namespace Ordering.Application.Features.Orders.Queries.GetOrders;

public record GetOrdersQueryResult(IEnumerable<Order> Orders);