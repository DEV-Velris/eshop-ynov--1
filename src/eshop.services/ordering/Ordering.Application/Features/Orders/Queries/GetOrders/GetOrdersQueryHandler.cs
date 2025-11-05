using BuildingBlocks.CQRS;
using Marten;
using Marten.Pagination;
using Ordering.Application.Features.Orders.Queries.GetOrder;
using Ordering.Domain.Models;

namespace Ordering.Application.Features.Orders.Queries.GetOrders;

public class GetOrdersQueryHandler(IDocumentSession documentSession)
    : IQueryHandler<GetOrdersQuery, GetOrdersQueryResult>
{
    public async Task<GetOrdersQueryResult> Handle(GetOrdersQuery request,
        CancellationToken cancellationToken)
    {

        var queryable = documentSession.Query<Order>().AsQueryable();

        var ordersQuery = await queryable.ToPagedListAsync(request.pageNumber, request.pageSize);


        return new GetOrdersQueryResult(ordersQuery);

    }
}