using BuildingBlocks.CQRS;
using Microsoft.Extensions.Logging;
using Ordering.Application.Features.Orders.Data;
using Ordering.Domain.Exceptions;

namespace Ordering.Application.Features.Orders.Commands.UpdateOrder;

/// <summary>
/// Handles the update order command, allowing the modification of an existing order in the system.
/// This handler retrieves the specified order, updates it with new values, and persists the changes
/// to the database. If the order does not exist, an exception is thrown.
/// </summary>
public class UpdateOrderCommandHandler(IOrderRepository orderRepository, ILogger<UpdateOrderCommandHandler> logger) 
    : ICommandHandler<UpdateOrderCommand, UpdateOrderCommandResult>
{
    public async Task<UpdateOrderCommandResult> Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating order {OrderId}", request.Order.Id);

        var existingOrder = await orderRepository.GetByIdAsync(request.Order.Id, cancellationToken);
        if (existingOrder == null)
        {
            var message = $"Order with ID {request.Order.Id} not found";
            logger.LogWarning(message);
            throw new OrderNotFoundException(message);
        }

        UpdateOrderCommandMapper.UpdateOrderWithNewValues(existingOrder, request.Order);
        await orderRepository.UpdateAsync(existingOrder, cancellationToken);
        
        logger.LogInformation("Order {OrderId} updated successfully", request.Order.Id);

        return new UpdateOrderCommandResult(true);
    }
}