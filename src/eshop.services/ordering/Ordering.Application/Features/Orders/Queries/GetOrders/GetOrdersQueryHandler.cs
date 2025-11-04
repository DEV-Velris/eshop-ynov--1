using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using Microsoft.Extensions.Logging;
using Ordering.Application.Features.Orders.Data;

namespace Ordering.Application.Features.Orders.Queries.GetOrders;

public class GetOrdersQueryHandler(
    IOrderRepository orderRepository,
    ILogger<GetOrdersQueryHandler> logger)
    : IQueryHandler<GetOrdersQuery, GetOrdersQueryResult>
{
    public async Task<GetOrdersQueryResult> Handle(
        GetOrdersQuery request, 
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving orders with pagination - PageIndex: {PageIndex}, PageSize: {PageSize}", 
            request.PaginationRequest.PageIndex, request.PaginationRequest.PageSize);

        var paginatedOrders = await orderRepository.GetOrdersAsync(request.PaginationRequest, cancellationToken);
        
        logger.LogInformation("Retrieved {Count} orders out of {Total} total orders", 
            paginatedOrders.Data.Count(), paginatedOrders.TotalCount);

        return new GetOrdersQueryResult(paginatedOrders);
    }
}