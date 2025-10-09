using Catalog.API.Models;

namespace Catalog.API.Features.Products.Queries.GetProducts;

/// <summary>
/// Represents the result of a <see cref="GetProductsQuery"/>, containing a list of products.
/// </summary>
/// <param name="Products">The list of products</param>
public record GetProductsQueryResult(IEnumerable<Product> Products);