using BuildingBlocks.CQRS;
using Catalog.API.Exceptions;
using Catalog.API.Models;
using Marten;
using Marten.Pagination;

namespace Catalog.API.Features.Products.Queries.GetProducts;

public class GetProductsQueryHandler(IDocumentSession documentSession)
    : IQueryHandler<GetProductsQuery, GetProductsQueryResult>
{
    public async Task<GetProductsQueryResult> Handle(GetProductsQuery request,
        CancellationToken cancellationToken)
    {
        
        var queryable = documentSession.Query<Product>().AsQueryable();
            
        if (request.name is not null)
        {
            queryable = queryable.Where(x => x.Name.ToLower().Contains(request.name.ToLower()));
        }
        if (request.minPrice is not null)
        {
            queryable = queryable.Where(x => x.Price >= request.minPrice);
        }
        if (request.maxPrice is not null)
        {
            queryable = queryable.Where(x => x.Price <= request.maxPrice);
        }
        
        var productsQuery = await queryable.ToPagedListAsync(request.pageNumber, request.pageSize);
        

        return new GetProductsQueryResult(productsQuery);
    }
}