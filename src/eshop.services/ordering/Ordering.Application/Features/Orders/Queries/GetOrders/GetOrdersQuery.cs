using BuildingBlocks.CQRS;

namespace Ordering.Application.Features.Orders.Queries.GetOrders;

public record GetOrdersQuery(int pageNumber, int pageSize) : IQuery<GetOrdersQueryResult>;