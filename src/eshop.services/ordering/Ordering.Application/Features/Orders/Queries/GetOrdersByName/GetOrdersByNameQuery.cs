using BuildingBlocks.CQRS;

namespace Ordering.Application.Features.Orders.Queries.GetOrdersByName;

public record GetOrdersByNameQuery(string OrderName) : IQuery<GetOrdersByNameQueryResult>;