namespace Catalog.API.Features.Products.Commands.DeleteProduct;

/// <summary>
/// Represents the result of executing the <see cref="DeleteProductCommand"/>.
/// </summary>
/// <param name="IsSuccessful"></param>
public record DeleteProductCommandResult(bool IsSuccessful);