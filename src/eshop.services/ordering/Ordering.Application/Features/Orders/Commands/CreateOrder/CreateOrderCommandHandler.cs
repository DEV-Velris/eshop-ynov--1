using BuildingBlocks.CQRS;
using BuildingBlocks.Messaging.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using Ordering.Application.Features.Orders.Data;

namespace Ordering.Application.Features.Orders.Commands.CreateOrder;

/// <summary>
/// Handles the creation of new orders and publishes OrderCreatedEvent for downstream processing.
/// </summary>
public class CreateOrderCommandHandler(
    IOrderingDbContext orderingDbContext, 
    IPublishEndpoint publishEndpoint,
    ILogger<CreateOrderCommandHandler> logger) : ICommandHandler<CreateOrderCommand, CreateOrderCommandResult>
{
    /// <summary>
    /// Handles the execution logic for creating an order command.
    /// </summary>
    /// <param name="request">The create order command containing the order details.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the result of the handling operation, containing the newly created order's ID.</returns>
    public async Task<CreateOrderCommandResult> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating order for customer {CustomerId}", request.Order.CustomerId);
        
        var order = CreateOrderCommandMapper.CreateNewOrderFromDto(request.Order);
        
        // Save order to database
        orderingDbContext.Orders.Add(order);
        await orderingDbContext.SaveChangesAsync(cancellationToken);
        
        logger.LogInformation("Order {OrderId} created successfully", order.Id.Value);
        
        // Publish OrderCreatedEvent for downstream processing
        var orderCreatedEvent = new OrderCreatedEvent
        {
            OrderId = order.Id.Value,
            CustomerId = order.CustomerId.Value,
            OrderName = order.OrderName.Value,
            TotalPrice = order.TotalPrice,
            OrderDate = order.CreatedAt ?? DateTime.UtcNow,
            CustomerName = $"{request.Order.ShippingAddress.FirstName} {request.Order.ShippingAddress.LastName}",
            CustomerEmail = request.Order.ShippingAddress.EmailAddress,
            OrderItems = [.. order.OrderItems.Select(oi => new OrderItemEvent
            {
                ProductId = oi.ProductId.Value,
                ProductName = oi.ProductName,
                Quantity = oi.Quantity,
                Price = oi.Price
            })]
        };

        await publishEndpoint.Publish(orderCreatedEvent, cancellationToken);
        
        logger.LogInformation("OrderCreatedEvent published for order {OrderId}", order.Id.Value);
        
        return new CreateOrderCommandResult(order.Id.Value);
    }
}