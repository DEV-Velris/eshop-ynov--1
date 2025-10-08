using BuildingBlocks.CQRS;

namespace Catalog.API.Features.Products.Queries.GetProductsByCategory;

/// <summary>
///  Represents a query to retrieve products by category.
/// This query returns a result of type <see cref="GetProductsByCategoryQueryResult"/>
/// </summary>
/// <param name="Category"></param>
public record GetProductsByCategoryQuery(string Category) : IQuery<GetProductsByCategoryQueryResult>;