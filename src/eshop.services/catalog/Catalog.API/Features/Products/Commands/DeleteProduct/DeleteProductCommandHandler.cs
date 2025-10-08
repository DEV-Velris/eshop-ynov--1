using BuildingBlocks.CQRS;
using Catalog.API.Exceptions;
using Catalog.API.Models;
using Mapster;
using Marten;

namespace Catalog.API.Features.Products.Commands.DeleteProduct;

/// <summary>
/// Handles the DeleteProduct command to remove an existing product from the system by deleting it through the provided document session.
/// </summary>
public class DeleteProductCommandHandler(IDocumentSession documentSession) : ICommandHandler<DeleteProductCommand, DeleteProductCommandResult>
{
    /// <summary>
    /// Handles the processing of the DeleteProduct command, which removes an existing product from the system.
    /// It ensures the product exists before attempting to delete it from the database.
    /// </summary>
    /// <param name="request">The DeleteProduct command containing the ID of the product to delete.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task representing the operation, containing the result of the command which includes the product ID.</returns>
    /// <exception cref="ProductNotFoundException">Thrown when the product to be deleted does not exist in the system.</exception>
    public async Task<DeleteProductCommandResult> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        // Vérifier l'existence du produit avant suppression
        var exists = await documentSession.Query<Product>()
            .AnyAsync(x => x.Id == request.Id, cancellationToken);

        if (!exists)
            throw new ProductNotFoundException(request.Id);

        // Supprimer par Id pour éviter d'avoir besoin d'un document tracké
        documentSession.Delete<Product>(request.Id);

        await documentSession.SaveChangesAsync(cancellationToken);

        return new DeleteProductCommandResult(request.Id);
    }
}