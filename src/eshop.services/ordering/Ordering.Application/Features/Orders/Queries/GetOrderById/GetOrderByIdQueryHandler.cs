using BuildingBlocks.CQRS;
using Microsoft.Extensions.Logging;
using Ordering.Application.Extensions;
using Ordering.Application.Features.Orders.Data;
using Ordering.Domain.Exceptions;

namespace Ordering.Application.Features.Orders.Queries.GetOrderById;

/// <summary>
/// Handles the GetOrderByIdQuery to retrieve a specific order by its ID.
/// </summary>
public class GetOrderByIdQueryHandler(IOrderRepository orderRepository, ILogger<GetOrderByIdQueryHandler> logger) 
    : IQueryHandler<GetOrderByIdQuery, GetOrderByIdQueryResult>
{
    public async Task<GetOrderByIdQueryResult> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching order with ID: {OrderId}", request.Id);

        var order = await orderRepository.GetByIdAsync(request.Id, cancellationToken);
        if (order == null)
        {
            logger.LogWarning("Order with ID {OrderId} not found", request.Id);
            throw new OrderNotFoundException(request.Id);
        }

        // Convert to DTO using the extension method
        var orderDto = order.ToOrderDto();
        
        logger.LogInformation("Order {OrderId} retrieved successfully", request.Id);
        
        return new GetOrderByIdQueryResult(orderDto);
    }
}