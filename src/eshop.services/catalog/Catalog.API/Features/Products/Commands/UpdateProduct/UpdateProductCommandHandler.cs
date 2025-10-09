using BuildingBlocks.CQRS;
using Catalog.API.Exceptions;
using Catalog.API.Models;
using Mapster;
using Marten;

namespace Catalog.API.Features.Products.Commands.UpdateProduct;

public class UpdateProductCommandHandler(IDocumentSession documentSession): ICommandHandler<UpdateProductCommand, UpdateProductCommandResult>
{
    public async Task<UpdateProductCommandResult> Handle(UpdateProductCommand request,
        CancellationToken cancellationToken)
    {
        var product = await documentSession.Query<Product>()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (product == null)
            throw new ProductNotFoundException(request.Id);

        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        product.Categories = request.Categories;
        product.ImageFile = request.ImageFile;
        
        documentSession.Update(product);
        await documentSession.SaveChangesAsync(cancellationToken);
        
        return new UpdateProductCommandResult(true);
    }
}