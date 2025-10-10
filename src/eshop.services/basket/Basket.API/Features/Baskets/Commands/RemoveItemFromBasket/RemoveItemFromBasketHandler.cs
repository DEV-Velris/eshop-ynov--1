using Basket.API.Data.Repositories;
using BuildingBlocks.CQRS;

namespace Basket.API.Features.Baskets.Commands.RemoveItemFromBasket;

/// <summary>
/// Validator for the <see cref="RemoveItemFromBasketCommand"/> class used to validate the data integrity of the command.
/// </summary>
/// <remarks>
/// Ensures that the required properties of the <see cref="RemoveItemFromBasketCommand"/> are properly populated before processing.
/// Performs validation checks on the <see cref="ShoppingCart"/> instance, including:
/// - Ensuring that the cart object itself is not null.
/// - Validating that the <see cref="ShoppingCart.UserName"/> is not empty.
/// </remarks>
public class RemoveItemFromBasketHandler(IBasketRepository repository) : ICommandHandler<RemoveItemFromBasketCommand, RemoveItemFromBasketCommandResult>
{
    public async Task<RemoveItemFromBasketCommandResult> Handle(RemoveItemFromBasketCommand command, CancellationToken cancellationToken)
    {
       await repository.RemoveItemFromBasketAsync(command.UserName, command.ProductId.ToString(), cancellationToken);
       return new RemoveItemFromBasketCommandResult(true, command.UserName);
    }
}
