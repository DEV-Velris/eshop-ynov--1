namespace Catalog.API.Features.Products.Commands.DeleteProduct;

/// <summary>
/// Represents the result of the DeleteProduct command execution.
/// </summary>
/// <param name="Id"></param>
public record DeleteProductCommandResult(Guid Id);
