using BuildingBlocks.CQRS;
using Microsoft.Extensions.Logging;
using Ordering.Application.Features.Orders.Data;
using Ordering.Domain.Exceptions;

namespace Ordering.Application.Features.Orders.Commands.DeleteOrder;

/// <summary>
/// Handles the deletion of orders from the system.
/// </summary>
public class DeleteOrderCommandHandler(IOrderRepository orderRepository, ILogger<DeleteOrderCommandHandler> logger) 
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

        var success = await orderRepository.DeleteAsync(request.OrderId, cancellationToken);
        if (!success)
        {
            var message = $"Order with ID {request.OrderId} not found or could not be deleted";
            logger.LogWarning(message);
            throw new OrderNotFoundException(request.OrderId);
        }
        
        logger.LogInformation("Order {OrderId} deleted successfully", request.OrderId);
        
        return new DeleteOrderCommandResult(true);
    }
}