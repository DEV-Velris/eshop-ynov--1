using Basket.API.Data.Repositories;
using BuildingBlocks.CQRS;

namespace Basket.API.Features.Baskets.Commands.AddItemToBasket;

/// <summary>
/// Validator for the <see cref="AddItemToBasketCommand"/> class used to validate the data integrity of the command.
/// </summary>
/// <remarks>
/// Ensures that the required properties of the <see cref="AddItemToBasketCommand"/> are properly populated before processing.
/// Performs validation checks on the <see cref="ShoppingCart"/> instance, including:
/// - Ensuring that the cart object itself is not null.
/// - Validating that the <see cref="ShoppingCart.UserName"/> is not empty.
/// </remarks>
public class AddItemToBasketHandler(IBasketRepository repository) : ICommandHandler<AddItemToBasketCommand, AddItemToBasketCommandResult>
{
    public async Task<AddItemToBasketCommandResult> Handle(AddItemToBasketCommand command, CancellationToken cancellationToken)
    {
        var shoppingCart = await repository.AddItemToBasketAsync(command.UserName, command.CartItem, cancellationToken);
        return new AddItemToBasketCommandResult(command.UserName, command.CartItem, shoppingCart);
    }
}
