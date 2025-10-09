using BuildingBlocks.CQRS;
using Catalog.API.Exceptions;
using Catalog.API.Models;
using Marten;

namespace Catalog.API.Features.Products.Commands.DeleteProduct;

/// <summary>
/// Handles the DeleteProduct command to remove an existing product from the system by utilizing the provided document session.
/// </summary>
public class DeleteProductCommandHandler(IDocumentSession documentSession)
    : ICommandHandler<DeleteProductCommand, DeleteProductCommandResult>
{
    /// <summary>
    /// Handles the processing of the DeleteProduct command, which removes a product from the system based on its identifier.
    /// </summary>
    /// <param name="request">The DeleteProduct command containig the details of the product to delete</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation</param>
    /// <returns>A task representing the operation, containing the result of the command</returns>
    /// <exception cref="ProductNotFoundException">Thrown when the product id is not present in database</exception>
    public async Task<DeleteProductCommandResult> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var productToDelete = await documentSession.LoadAsync<Product>(request.Id, cancellationToken);

        if (productToDelete is null)
        {
            throw new ProductNotFoundException(request.Id);
        }
        
        documentSession.Delete(productToDelete);
        await documentSession.SaveChangesAsync(cancellationToken);
        
        return new DeleteProductCommandResult(true);
    }
}