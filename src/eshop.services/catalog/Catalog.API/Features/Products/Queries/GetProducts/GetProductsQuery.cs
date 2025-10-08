using BuildingBlocks.CQRS;

namespace Catalog.API.Features.Products.Queries.GetProducts;

/// <summary>
/// Represents a query to retrieve a paginated list of products from the catalog.
/// </summary>
/// <param name="PageNumber">The page number</param>
/// <param name="PageSize">The page size</param>
public record GetProductsQuery(int PageNumber, int PageSize) : IQuery<GetProductsQueryResult>;