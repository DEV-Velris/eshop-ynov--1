using BuildingBlocks.Pagination;
using Ordering.Application.Features.Orders.Dtos;

namespace Ordering.Application.Features.Orders.Queries.GetOrders;

public record GetOrdersQueryResult(PaginatedResult<OrderDto> Orders);