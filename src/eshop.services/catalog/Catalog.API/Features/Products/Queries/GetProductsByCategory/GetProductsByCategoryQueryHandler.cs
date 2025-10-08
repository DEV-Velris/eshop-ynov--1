using BuildingBlocks.CQRS;
using Catalog.API.Models;
using Marten;

namespace Catalog.API.Features.Products.Queries.GetProductsByCategory;

/// <summary>
/// Handles the execution of the <see cref="GetProductsByCategoryQuery"/> and retrieves the corresponding
/// </summary>
/// <param name="documentSession">The document session</param>
public class GetProductsByCategoryQueryHandler(IDocumentSession documentSession)
    : IQueryHandler<GetProductsByCategoryQuery, GetProductsByCategoryQueryResult>
{
    /// <summary>
    /// Handles the execution of the GetProductsByCategoryQuery and retrieves the associated product data.
    /// </summary>
    /// <param name="request">The query containing the category name</param>
    /// <param name="cancellationToken">A token monitor for cancellation requests</param>
    /// <returns>A task that represents the asynchronous operation, containing the result of the query, including the product data</returns>
    public async Task<GetProductsByCategoryQueryResult> Handle(GetProductsByCategoryQuery request,
        CancellationToken cancellationToken)
    {
        var products = await documentSession.Query<Product>()
            .Where(p => p.Categories.Any(c => c.Equals(request.Category, StringComparison.InvariantCultureIgnoreCase)))
            .ToListAsync(cancellationToken);
        
        return new GetProductsByCategoryQueryResult(products);
    }
}