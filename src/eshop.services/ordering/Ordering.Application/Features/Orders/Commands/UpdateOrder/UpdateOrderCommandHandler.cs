using BuildingBlocks.CQRS;
using BuildingBlocks.Messaging.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using Ordering.Application.Features.Orders.Data;
using Ordering.Domain.Exceptions;

namespace Ordering.Application.Features.Orders.Commands.UpdateOrder;

/// <summary>
/// Handles the update order command, allowing the modification of an existing order in the system.
/// This handler retrieves the specified order, updates it with new values, and persists the changes
/// to the database. If the order does not exist, an exception is thrown.
/// </summary>
public class UpdateOrderCommandHandler(
    IOrderRepository orderRepository, 
    IPublishEndpoint publishEndpoint,
    ILogger<UpdateOrderCommandHandler> logger) 
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

        logger.LogInformation("Domain events before update: {EventCount}", existingOrder.DomainEvents.Count);
        
        UpdateOrderCommandMapper.UpdateOrderWithNewValues(existingOrder, request.Order);
        
        logger.LogInformation("Domain events after update: {EventCount}", existingOrder.DomainEvents.Count);
        logger.LogInformation("Domain events: {Events}", string.Join(", ", existingOrder.DomainEvents.Select(e => e.GetType().Name)));
        
        await orderRepository.UpdateAsync(existingOrder, cancellationToken);
        
        logger.LogInformation("Order {OrderId} updated successfully", request.Order.Id);

        var orderUpdatedEvent = new OrderUpdatedEvent
        {
            OrderId = existingOrder.Id.Value,
            CustomerId = existingOrder.CustomerId.Value,
            OrderName = existingOrder.OrderName.Value,
            OrderStatus = existingOrder.OrderStatus.ToString(),
            PreviousStatus = "Updated",
            TotalPrice = existingOrder.TotalPrice,
            UpdatedDate = DateTime.UtcNow,
            CustomerName = $"{existingOrder.ShippingAddress.FirstName} {existingOrder.ShippingAddress.LastName}",
            CustomerEmail = existingOrder.ShippingAddress.EmailAddress,
            OrderItems = existingOrder.OrderItems.Select(oi => new OrderItemEvent
            {
                ProductId = oi.ProductId.Value,
                ProductName = oi.ProductName,
                Quantity = oi.Quantity,
                Price = oi.Price
            }).ToList()
        };

        await publishEndpoint.Publish(orderUpdatedEvent, cancellationToken);
        
        logger.LogInformation("🚀 OrderUpdatedEvent published for order {OrderId}", request.Order.Id);

        return new UpdateOrderCommandResult(true);
    }
}