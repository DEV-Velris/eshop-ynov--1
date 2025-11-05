namespace BuildingBlocks.Messaging.Events;

/// <summary>
/// Event published when an order status is deleted in the system.
/// This event triggers downstream processes like notifications, tracking updates, etc.
/// </summary>
public record OrderDeletedEvent : IntegrationEvent
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public string OrderName { get; set; } = null!;
    public string OrderStatus { get; set; } = null!;
    public string PreviousStatus { get; set; } = null!;
    public decimal TotalPrice { get; set; }
    public DateTime UpdatedDate { get; set; }
    
    // Customer Info for notifications
    public string CustomerName { get; set; } = null!;
    public string CustomerEmail { get; set; } = null!;
    
    // Order Items for reference
    public List<OrderItemEvent> OrderItems { get; set; } = new();
}