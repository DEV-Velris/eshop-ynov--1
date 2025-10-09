using BuildingBlocks.CQRS;
using Catalog.API.Exceptions;
using Catalog.API.Models;
using Marten;
using Marten.Pagination;

namespace Catalog.API.Features.Products.Queries.GetProductsByCategory;

public class GetProductsByCategoryQueryHandler(IDocumentSession documentSession)
    : IQueryHandler<GetProductsByCategoryQuery, GetProductsByCategoryQueryResult>
{
    public async Task<GetProductsByCategoryQueryResult> Handle(GetProductsByCategoryQuery request,
        CancellationToken cancellationToken)
    {
        var products = await documentSession
            .Query<Product>()
            .Where(p => p.Categories.Any(c => c.Equals(request.category, StringComparison.OrdinalIgnoreCase)))
            .ToListAsync(cancellationToken);
        
        return new GetProductsByCategoryQueryResult(products);
    }
}