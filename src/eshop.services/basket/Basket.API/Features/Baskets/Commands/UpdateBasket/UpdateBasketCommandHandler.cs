using Basket.API.Data.Repositories;
using Basket.API.Models;
using BuildingBlocks.CQRS;

namespace Basket.API.Features.Baskets.Commands.UpdateBasket;

/// <summary>
/// Handles the updating of a shopping basket by processing the UpdateBasketCommand.
/// Implements the <see cref="ICommandHandler{UpdateBasketCommand, UpdateBasketCommandResult}"/> interface.
/// </summary>
public class UpdateBasketCommandHandler(IBasketRepository repository) : ICommandHandler<UpdateBasketCommand, UpdateBasketCommandResult>
{
    /// <summary>
    /// Handles the request to update a shopping basket.
    /// </summary>
    /// <param name="request">The UpdateBasketCommand containing the details of the shopping basket to be updated.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the operation to complete.</param>
    /// <returns>A task representing the asynchronous operation, returning a UpdateBasketCommandResult that indicates the success of the operation and includes the UserName of the updated basket.</returns>
    public async Task<UpdateBasketCommandResult> Handle(UpdateBasketCommand request,
        CancellationToken cancellationToken)
    {
        var item = request.Item;
        var userName = request.UserName;

        var basketCart = await repository.UpdateBasketAsync(userName, item, cancellationToken)
            .ConfigureAwait(false);

        return new UpdateBasketCommandResult(true, basketCart.UserName);
    }
}