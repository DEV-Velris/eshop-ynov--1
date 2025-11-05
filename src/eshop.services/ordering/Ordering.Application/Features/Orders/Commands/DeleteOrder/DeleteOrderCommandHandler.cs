using BuildingBlocks.CQRS;
using BuildingBlocks.Messaging.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using Ordering.Application.Features.Orders.Data;
using Ordering.Domain.Exceptions;

namespace Ordering.Application.Features.Orders.Commands.DeleteOrder;

/// <summary>
/// Handles the deletion of orders from the system.
/// </summary>
public class DeleteOrderCommandHandler(
    IOrderRepository orderRepository,
    ILogger<DeleteOrderCommandHandler> logger,
    IPublishEndpoint publishEndpoint
)
     
    : ICommandHandler<DeleteOrderCommand, DeleteOrderCommandResult>
{
    /// <summary>
    /// Handles the operation for deleting an order based on the provided command.
    /// </summary>
    /// <param name="request">The command containing the order identifier to delete.</param>
    /// <param name="cancellationToken">Token to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="DeleteOrderCommandResult"/> containing the result of the delete operation.</returns>
    /// <exception cref="OrderNotFoundException">
    /// Thrown when an order with the specified identifier is not found.
    /// </exception>
    public async Task<DeleteOrderCommandResult> Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting order {OrderId}", request.OrderId);

        var order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order == null)
        {
            var message = $"Order with ID {request.OrderId} not found";
            logger.LogWarning(message);
            throw new OrderNotFoundException(message);
        }

        var success = await orderRepository.DeleteAsync(request.OrderId, cancellationToken);
        if (!success)
        {
            var message = $"Order with ID {request.OrderId} not found or could not be deleted";
            logger.LogWarning(message);
            throw new OrderNotFoundException(request.OrderId);
        }

        var orderDeletedEvent = new OrderDeletedEvent
        {
            OrderId = order.Id.Value,
            CustomerId = order.CustomerId.Value,
            OrderName = order.OrderName.Value,
            OrderStatus = order.OrderStatus.ToString(),
            PreviousStatus = "Deleted",
            TotalPrice = order.TotalPrice,
            UpdatedDate = DateTime.UtcNow,
            CustomerName = $"{order.ShippingAddress.FirstName} {order.ShippingAddress.LastName}",
            CustomerEmail = order.ShippingAddress.EmailAddress,
            OrderItems = order.OrderItems.Select(oi => new OrderItemEvent
            {
                ProductId = oi.ProductId.Value,
                ProductName = oi.ProductName,
                Quantity = oi.Quantity,
                Price = oi.Price
            }).ToList()

        };

        await publishEndpoint.Publish(orderDeletedEvent, cancellationToken);
        
        logger.LogInformation("Order {OrderId} deleted successfully", request.OrderId);
        
        return new DeleteOrderCommandResult(true);
    }
}