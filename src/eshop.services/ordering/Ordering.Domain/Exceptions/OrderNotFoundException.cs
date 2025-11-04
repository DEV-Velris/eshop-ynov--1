namespace Ordering.Domain.Exceptions;

/// <summary>
/// Exception thrown when an order is not found in the system.
/// </summary>
public class OrderNotFoundException : DomainException
{
    public OrderNotFoundException(string message) : base(message)
    {
    }

    public OrderNotFoundException(Guid orderId) : base($"Order with ID {orderId} was not found.")
    {
    }
}