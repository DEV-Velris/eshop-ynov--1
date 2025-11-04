namespace BuildingBlocks.Messaging.Events;

/// <summary>
/// Event published when a new order is successfully created in the system.
/// This event triggers downstream processes like inventory updates, notifications, etc.
/// </summary>
public record OrderCreatedEvent : IntegrationEvent
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public string OrderName { get; set; } = null!;
    public decimal TotalPrice { get; set; }
    public DateTime OrderDate { get; set; }
    
    // Customer Info for notifications
    public string CustomerName { get; set; } = null!;
    public string CustomerEmail { get; set; } = null!;
    
    // Order Items for inventory management
    public List<OrderItemEvent> OrderItems { get; set; } = new();
}

/// <summary>
/// Represents an order item within the OrderCreatedEvent.
/// </summary>
public record OrderItemEvent
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}