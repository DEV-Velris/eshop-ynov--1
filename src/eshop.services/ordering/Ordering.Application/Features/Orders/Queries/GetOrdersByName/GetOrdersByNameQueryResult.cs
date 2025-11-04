using Ordering.Application.Features.Orders.Dtos;

namespace Ordering.Application.Features.Orders.Queries.GetOrdersByName;

public record GetOrdersByNameQueryResult(IEnumerable<OrderDto> Orders);