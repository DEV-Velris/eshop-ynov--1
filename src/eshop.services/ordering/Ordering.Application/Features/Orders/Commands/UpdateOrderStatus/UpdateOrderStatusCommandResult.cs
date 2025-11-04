namespace Ordering.Application.Features.Orders.Commands.UpdateOrderStatus;

/// <summary>
/// Represents the result of updating an order's status.
/// </summary>
/// <param name="IsSuccess">Indicates whether the status update was successful.</param>
public record UpdateOrderStatusCommandResult(bool IsSuccess);