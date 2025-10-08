using BuildingBlocks.CQRS;
using Catalog.API.Exceptions;
using Catalog.API.Models;
using Marten;

namespace Catalog.API.Features.Products.Commands.UpdateProduct;

/// <summary>
/// Handles the UpdateProduct command to update an existing product in the system.
/// </summary>
public class UpdateProductCommandHandler(IDocumentSession documentSession) : ICommandHandler<UpdateProductCommand, UpdateProductCommandResult>
{
    /// <summary>
    /// Handles the processing of the UpdateProduct command, which updates an existing product in the system.
    /// </summary>
    /// <param name="request">The UpdateProduct command containing the details of the product to update</param>
    /// <param name="cancellationToken">A token that ca be used to cancel operation.</param>
    /// <returns>A task representing the operation, containing the result of the command</returns>
    /// <exception cref="ProductNotFoundException">Thrown when a product with the specified ID does not exist in the system.</exception>
    public async Task<UpdateProductCommandResult> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var productToUpdate = await documentSession.LoadAsync<Product>(request.Id, cancellationToken);

        if (productToUpdate is null)
        {
            throw new ProductNotFoundException(request.Id);
        }
        
        productToUpdate.Name = request.Name;
        productToUpdate.Description = request.Description;
        productToUpdate.Price = request.Price;
        productToUpdate.ImageFile = request.ImageFile;
        productToUpdate.Categories = request.Categories;
        
        documentSession.Update(productToUpdate);
        await documentSession.SaveChangesAsync(cancellationToken);
        
        return new UpdateProductCommandResult(true);
    }
}