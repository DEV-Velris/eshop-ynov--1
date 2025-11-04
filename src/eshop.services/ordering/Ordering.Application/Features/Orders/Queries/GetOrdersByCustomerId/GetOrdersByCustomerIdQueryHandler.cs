using MediatR;
using Microsoft.Extensions.Logging;
using Ordering.Application.Features.Orders.Data;

namespace Ordering.Application.Features.Orders.Queries.GetOrdersByCustomerId;

public class GetOrdersByCustomerIdQueryHandler(
    IOrderRepository orderRepository,
    ILogger<GetOrdersByCustomerIdQueryHandler> logger)
    : IRequestHandler<GetOrdersByCustomerIdQuery, GetOrdersByCustomerIdQueryResult>
{
    public async Task<GetOrdersByCustomerIdQueryResult> Handle(
        GetOrdersByCustomerIdQuery request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling GetOrdersByCustomerIdQuery for CustomerId: {CustomerId}", request.CustomerId);

        var orders = await orderRepository.GetOrdersByCustomerIdAsync(request.CustomerId);
        var ordersList = orders.ToList();
        
        logger.LogInformation("Retrieved {Count} orders for CustomerId: {CustomerId}", ordersList.Count, request.CustomerId);

        return new GetOrdersByCustomerIdQueryResult(ordersList);
    }
}