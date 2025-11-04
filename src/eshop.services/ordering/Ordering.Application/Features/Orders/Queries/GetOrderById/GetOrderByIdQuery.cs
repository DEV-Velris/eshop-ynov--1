using BuildingBlocks.CQRS;
using Ordering.Application.Features.Orders.Dtos;

namespace Ordering.Application.Features.Orders.Queries.GetOrderById;

/// <summary>
/// Represents a query to retrieve an order by its unique identifier.
/// </summary>
/// <param name="Id">The unique identifier of the order to retrieve.</param>
public record GetOrderByIdQuery(Guid Id) : IQuery<GetOrderByIdQueryResult>;