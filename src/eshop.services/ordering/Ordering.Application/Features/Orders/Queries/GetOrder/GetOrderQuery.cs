using BuildingBlocks.CQRS;

namespace Ordering.Application.Features.Orders.Queries.GetOrder;

public record GetOrderQuery(string name) : IQuery<GetOrderQueryResult>;