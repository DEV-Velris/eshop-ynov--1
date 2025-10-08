using BuildingBlocks.CQRS;

namespace Catalog.API.Features.Products.Commands.DeleteProduct;

/// <summary>
/// Represents the command to delete an existing product.
/// </summary>
public class DeleteProductCommand : ICommand<DeleteProductCommandResult>
{
    /// <summary>
    /// Gets or sets the unique identifier of the product to be deleted.
    /// </summary>
    public Guid Id { get; set; }
}