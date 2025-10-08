using BuildingBlocks.CQRS;
using Catalog.API.Exceptions;
using Catalog.API.Models;
using Marten;

namespace Catalog.API.Features.Products.Queries.GetProductsByCategory;

/// <summary>
/// Handles the retrieval of products by category.
/// </summary>
/// <param name="documentSession"></param>
public class GetProductsByCategoryQueryHandler(IDocumentSession documentSession) : IQueryHandler<GetProductsByCategoryQuery, GetProductsByCategoryQueryResult>
{
    /// <summary>
    /// Handles the retrieval of products by category.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ProductsNotFoundByCategoryException"></exception>
    public async Task<GetProductsByCategoryQueryResult> Handle(GetProductsByCategoryQuery request, CancellationToken cancellationToken)
    {
        var products = await documentSession.Query<Product>()
            .Where(x => x.Categories.Contains(request.Category))
            .ToListAsync(cancellationToken);
        if (products is null || !products.Any())
        {
            throw new ProductsNotFoundByCategoryException(request.Category);
        }

        return new GetProductsByCategoryQueryResult(products);
    }
}