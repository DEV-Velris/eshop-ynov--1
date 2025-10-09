using BuildingBlocks.CQRS;
using Marten;

namespace Catalog.API.Features.Products.Commands.DeleteProduct;

public class DeleteProductCommandHandler(IDocumentSession documentSession): ICommandHandler<DeleteProductCommand, DeleteProductCommandResult>
{
    public async Task<DeleteProductCommandResult> Handle(DeleteProductCommand request,
        CancellationToken cancellationToken)
    {
        var product = await documentSession.LoadAsync<Models.Product>(request.Id, cancellationToken);
        if (product is null)
        {
            return new DeleteProductCommandResult(false);
        }

        documentSession.Delete(product);
        await documentSession.SaveChangesAsync(cancellationToken);

        return new DeleteProductCommandResult(true);
    }
}