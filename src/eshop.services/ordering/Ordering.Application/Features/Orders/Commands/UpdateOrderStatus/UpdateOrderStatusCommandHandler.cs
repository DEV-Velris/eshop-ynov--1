using BuildingBlocks.CQRS;
using BuildingBlocks.Messaging.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using Ordering.Application.Features.Orders.Data;
using Ordering.Domain.Exceptions;

namespace Ordering.Application.Features.Orders.Commands.UpdateOrderStatus;

/// <summary>
/// Handles the UpdateOrderStatusCommand to update the status of an existing order.
/// </summary>
public class UpdateOrderStatusCommandHandler(
    IOrderRepository orderRepository, 
    IPublishEndpoint publishEndpoint,
    ILogger<UpdateOrderStatusCommandHandler> logger) 
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

        // Sauvegarder l'ancien statut avant la mise à jour
        var previousStatus = existingOrder.OrderStatus;
        
        // Update only the order status
        existingOrder.Update(
            existingOrder.OrderName,
            existingOrder.ShippingAddress,
            request.OrderStatus,
            existingOrder.BillingAddress,
            existingOrder.Payment);

        await orderRepository.UpdateAsync(existingOrder, cancellationToken);
        
        var orderUpdatedEvent = new OrderUpdatedEvent
        {
            OrderId = existingOrder.Id.Value,
            CustomerId = existingOrder.CustomerId.Value,
            OrderName = existingOrder.OrderName.Value,
            OrderStatus = request.OrderStatus.ToString(),
            PreviousStatus = previousStatus.ToString(),
            TotalPrice = existingOrder.TotalPrice,
            UpdatedDate = DateTime.UtcNow,
            CustomerName = $"{existingOrder.ShippingAddress.FirstName} {existingOrder.ShippingAddress.LastName}",
            CustomerEmail = existingOrder.ShippingAddress.EmailAddress,
            OrderItems = existingOrder.OrderItems.Select(oi => new OrderItemEvent
            {
                ProductId = oi.ProductId.Value,
                ProductName = $"Produit #{oi.ProductId.Value}",
                Quantity = oi.Quantity,
                Price = oi.Price
            }).ToList()
        };

        await publishEndpoint.Publish(orderUpdatedEvent, cancellationToken);
        
        logger.LogInformation("Order {OrderId} status updated successfully to {OrderStatus} and OrderUpdatedEvent published", 
            request.OrderId, request.OrderStatus);

        return new UpdateOrderStatusCommandResult(true);
    }
}