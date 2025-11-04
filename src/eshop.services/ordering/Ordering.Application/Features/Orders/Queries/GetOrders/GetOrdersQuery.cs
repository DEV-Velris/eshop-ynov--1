using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;

namespace Ordering.Application.Features.Orders.Queries.GetOrders;

public record GetOrdersQuery(PaginationRequest PaginationRequest) : IQuery<GetOrdersQueryResult>;