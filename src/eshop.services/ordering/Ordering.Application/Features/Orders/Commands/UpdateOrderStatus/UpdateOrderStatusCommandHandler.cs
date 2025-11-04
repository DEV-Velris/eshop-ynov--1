using BuildingBlocks.CQRS;
using Microsoft.Extensions.Logging;
using Ordering.Application.Features.Orders.Data;
using Ordering.Domain.Exceptions;

namespace Ordering.Application.Features.Orders.Commands.UpdateOrderStatus;

/// <summary>
/// Handles the UpdateOrderStatusCommand to update the status of an existing order.
/// </summary>
public class UpdateOrderStatusCommandHandler(IOrderRepository orderRepository, ILogger<UpdateOrderStatusCommandHandler> logger) 
    : ICommandHandler<UpdateOrderStatusCommand, UpdateOrderStatusCommandResult>
{
    public async Task<UpdateOrderStatusCommandResult> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating status of order {OrderId} to {OrderStatus}", request.OrderId, request.OrderStatus);

        var existingOrder = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (existingOrder == null)
        {
            var message = $"Order with ID {request.OrderId} not found";
            logger.LogWarning(message);
            throw new OrderNotFoundException(message);
        }

        // Update only the order status
        existingOrder.Update(
            existingOrder.OrderName,
            existingOrder.ShippingAddress,
            request.OrderStatus,
            existingOrder.BillingAddress,
            existingOrder.Payment);

        await orderRepository.UpdateAsync(existingOrder, cancellationToken);
        
        logger.LogInformation("Order {OrderId} status updated successfully to {OrderStatus}", request.OrderId, request.OrderStatus);

        return new UpdateOrderStatusCommandResult(true);
    }
}