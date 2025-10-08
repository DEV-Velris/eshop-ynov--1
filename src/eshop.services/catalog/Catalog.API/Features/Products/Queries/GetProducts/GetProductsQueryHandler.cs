using BuildingBlocks.CQRS;
using Catalog.API.Models;
using Marten;

namespace Catalog.API.Features.Products.Queries.GetProducts;

/// <summary>
/// Handles the execution of the <see cref="GetProductsQuery"/> and retrieves the corresponding
/// </summary>
/// <param name="documentSession">The document session</param>
public class GetProductsQueryHandler(IDocumentSession documentSession) : IQueryHandler<GetProductsQuery, GetProductsQueryResult>
{
    /// <summary>
    /// Handles the execution of the GetProductsQuery and retrieves the associated product data.
    /// </summary>
    /// <param name="request">Contains the request parameters</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns></returns>
    public async Task<GetProductsQueryResult> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await documentSession.Query<Product>()
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);
        
        return new GetProductsQueryResult(products);
    }
}