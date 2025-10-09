using BuildingBlocks.CQRS;

namespace Catalog.API.Features.Products.Queries.ExportProducts;

/// <summary>
/// Query to export products
/// </summary>
public record ExportProductsQuery() : IQuery<ExportProductsQueryResult>;