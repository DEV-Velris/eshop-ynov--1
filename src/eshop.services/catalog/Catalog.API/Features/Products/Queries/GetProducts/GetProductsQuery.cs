using BuildingBlocks.CQRS;

namespace Catalog.API.Features.Products.Queries.GetProducts;

public record GetProductsQuery(int pageNumber, int pageSize, string? name, decimal? minPrice, decimal? maxPrice) : IQuery<GetProductsQueryResult>;