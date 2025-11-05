using BuildingBlocks.CQRS;
using Marten;
using Ordering.Application.Exceptions;
using Ordering.Domain.Models;

namespace Ordering.Application.Features.Orders.Queries.GetOrder;

public class GetOrderQueryHandler(IDocumentSession documentSession) 
    : IQueryHandler<GetOrderQuery, GetOrderQueryResult>
{
    public async Task<GetOrderQueryResult> Handle(GetOrderQuery request,
        CancellationToken cancellationToken)
    {
        var order = await documentSession.LoadAsync<Order>(request.name, cancellationToken);
        if (order is null)
        {
            throw new OrderNotFoundException(request.name);
        }
        
        return new GetOrderQueryResult(order);
    }
}