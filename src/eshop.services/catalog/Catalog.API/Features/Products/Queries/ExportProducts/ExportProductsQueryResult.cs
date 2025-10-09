namespace Catalog.API.Features.Products.Queries.ExportProducts;

/// <summary>
/// Result of exporting products
/// </summary>
/// <param name="Content">The content of the sheet</param>
/// <param name="ContentType">The contentType</param>
/// <param name="FileName">The FileName</param>
public record ExportProductsQueryResult(byte[] Content, string ContentType, string FileName);