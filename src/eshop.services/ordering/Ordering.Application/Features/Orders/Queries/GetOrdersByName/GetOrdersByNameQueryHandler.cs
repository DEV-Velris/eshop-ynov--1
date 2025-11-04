using BuildingBlocks.CQRS;
using Microsoft.Extensions.Logging;
using Ordering.Application.Features.Orders.Data;

namespace Ordering.Application.Features.Orders.Queries.GetOrdersByName;

public class GetOrdersByNameQueryHandler(
    IOrderRepository orderRepository,
    ILogger<GetOrdersByNameQueryHandler> logger)
    : IQueryHandler<GetOrdersByNameQuery, GetOrdersByNameQueryResult>
{
    public async Task<GetOrdersByNameQueryResult> Handle(
        GetOrdersByNameQuery request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving orders with name: {OrderName}", request.OrderName);

        var orders = await orderRepository.GetOrdersByNameAsync(request.OrderName, cancellationToken);
        var ordersList = orders.ToList();
        
        logger.LogInformation("Found {Count} orders with name: {OrderName}", ordersList.Count, request.OrderName);

        return new GetOrdersByNameQueryResult(ordersList);
    }
}