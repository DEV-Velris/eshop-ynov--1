using BuildingBlocks.Messaging.Events;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using Ordering.Application.Extensions;
using Ordering.Domain.Events;
using DomainOrderUpdatedEvent = Ordering.Domain.Events.OrderUpdatedEvent;
using IntegrationOrderUpdatedEvent = BuildingBlocks.Messaging.Events.OrderUpdatedEvent;

namespace Ordering.Application.Features.Orders.EventHandlers.Domain;

/// <summary>
/// Handles the domain event for an order being updated.
/// This handler is responsible for processing the <see cref="OrderUpdatedEvent"/>
/// and publishing an integration event based on the updated order details.
/// </summary>
public class OrderUpdatedEventHandler(
    IPublishEndpoint publishEndpoint, 
    IFeatureManager featureManager, 
    ILogger<OrderUpdatedEventHandler> logger) : INotificationHandler<DomainOrderUpdatedEvent>
{
    /// <summary>
    /// Handles the domain event when an order is updated.
    /// </summary>
    /// <param name="notification">The <see cref="DomainOrderUpdatedEvent"/> containing details of the updated order.</param>
    /// <param name="cancellationToken">A cancellation token to observe while performing the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task Handle(DomainOrderUpdatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Domain Event Handled: {DomainEvent}", notification.GetType().Name);

        var isFeatureEnabled = await featureManager.IsEnabledAsync("OrderFulfilment");
        logger.LogInformation("OrderFulfilment feature enabled: {IsEnabled}", isFeatureEnabled);

        if (isFeatureEnabled)
        {
            var orderUpdatedIntegrationEvent = new IntegrationOrderUpdatedEvent
            {
                OrderId = notification.Order.Id.Value,
                CustomerId = notification.Order.CustomerId.Value,
                OrderName = notification.Order.OrderName.Value,
                OrderStatus = notification.Order.OrderStatus.ToString(),
                PreviousStatus = "Updated",
                TotalPrice = notification.Order.TotalPrice,
                UpdatedDate = DateTime.UtcNow,
                CustomerName = $"{notification.Order.ShippingAddress.FirstName} {notification.Order.ShippingAddress.LastName}",
                CustomerEmail = notification.Order.ShippingAddress.EmailAddress,
                OrderItems = notification.Order.OrderItems.Select(oi => new OrderItemEvent
                {
                    ProductId = oi.ProductId.Value,
                    ProductName = oi.ProductName,
                    Quantity = oi.Quantity,
                    Price = oi.Price
                }).ToList()
            };

            logger.LogInformation("About to publish integration event for order {OrderId}", 
                notification.Order.Id.Value);
            
            await publishEndpoint.Publish(orderUpdatedIntegrationEvent, cancellationToken);
            
            logger.LogInformation("✅ Integration Event Published: OrderUpdatedEvent for order {OrderId}", 
                notification.Order.Id.Value);
        }
    }
}