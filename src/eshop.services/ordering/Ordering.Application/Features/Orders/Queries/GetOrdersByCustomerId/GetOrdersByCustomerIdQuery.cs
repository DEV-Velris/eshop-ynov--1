using MediatR;
using Ordering.Application.Features.Orders.Dtos;

namespace Ordering.Application.Features.Orders.Queries.GetOrdersByCustomerId;

public record GetOrdersByCustomerIdQuery(Guid CustomerId) : IRequest<GetOrdersByCustomerIdQueryResult>;