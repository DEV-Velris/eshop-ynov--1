using BuildingBlocks.CQRS;

namespace Catalog.API.Features.Products.Queries.GetProductsByCategory;

public record GetProductsByCategoryQuery(string category) : IQuery<GetProductsByCategoryQueryResult>;