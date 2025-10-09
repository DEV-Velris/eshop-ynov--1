namespace Catalog.API.Features.Products.Commands.ImportProducts;

/// <summary>
/// Result of importing products from a CSV file
/// </summary>
/// <param name="InsertedCount">The number of inserted product</param>
/// <param name="UpdatedCount">The number of modified product</param>
/// <param name="FailedCount">The number of failed importation</param>
/// <param name="Errors">The errors</param>
public sealed record ImportProductsCommandResult(int InsertedCount, int UpdatedCount, int FailedCount, IReadOnlyList<string> Errors);