using BuildingBlocks.CQRS;

namespace Catalog.API.Features.Products.Queries.GetProducts;

/// <summary>
/// Represents a query to retrieve a paginated list of products from the catalog.
/// </summary>
/// <param name="PageNumber">The page number</param>
/// <param name="PageSize">The page size</param>
/// <param name="Category">The category filter (optional)</param>
/// <param name="Name">The name filter (optional)</param>
/// <param name="MinPrice">The minimum price filter (optional)</param>
/// <param name="MaxPrice">The maximum price filter (optional)</param>
public record GetProductsQuery(
    int PageNumber,
    int PageSize,
    string? Category = null,
    string? Name = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null
) : IQuery<GetProductsQueryResult>;