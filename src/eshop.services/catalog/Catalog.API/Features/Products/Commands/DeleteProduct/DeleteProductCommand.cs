using BuildingBlocks.CQRS;

namespace Catalog.API.Features.Products.Commands.DeleteProduct;

/// <summary>
/// Represents a command to delete an existing product from the catalog.
/// </summary>
/// <param name="Id">The id of the product to delete</param>
public record DeleteProductCommand(Guid Id) : ICommand<DeleteProductCommandResult>;