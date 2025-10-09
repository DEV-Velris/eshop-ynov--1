using Catalog.API.Models;

namespace Catalog.API.Features.Products.Queries.GetProductsByCategory;

public record GetProductsByCategoryQueryResult(IEnumerable<Product> Products);