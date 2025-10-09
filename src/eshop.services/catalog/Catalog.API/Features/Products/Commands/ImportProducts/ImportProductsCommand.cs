using BuildingBlocks.CQRS;

namespace Catalog.API.Features.Products.Commands.ImportProducts;

/// <summary>
/// Import products from a CSV file
/// </summary>
/// <param name="File">The Excel file used to import Products</param>
public sealed record ImportProductsCommand(IFormFile File) : ICommand<ImportProductsCommandResult>;