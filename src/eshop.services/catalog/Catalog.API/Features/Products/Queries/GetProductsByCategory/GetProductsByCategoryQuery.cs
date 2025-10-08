using BuildingBlocks.CQRS;

namespace Catalog.API.Features.Products.Queries.GetProductsByCategory;

/// <summary>
/// Represents a query to retrieve products based on a specified category.
/// </summary>
/// <param name="Category">The requested category name</param>
public record GetProductsByCategoryQuery(string Category) : IQuery<GetProductsByCategoryQueryResult>;