namespace Ordering.Application.Features.Orders.Dtos;

public record OrderItemDto(Guid OrderId, Guid ProductId, string ProductName, int Quantity, decimal Price);