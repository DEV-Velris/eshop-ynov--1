using Catalog.API.Models;

namespace Catalog.API.Features.Products.Queries.GetProductsByCategory;

/// <summary>
/// Represents the result of a query to retrieve products by category.
/// Contains a collection of <see cref="Product"/> items that belong to the specified category.
/// </summary>
/// <param name="Products"></param>
public record GetProductsByCategoryQueryResult(IEnumerable<Product> Products);