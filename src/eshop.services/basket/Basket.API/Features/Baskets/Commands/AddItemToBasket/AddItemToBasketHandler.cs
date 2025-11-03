using Basket.API.Data.Repositories;
using BuildingBlocks.CQRS;
using Discount.Grpc;

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
public class AddItemToBasketHandler(IBasketRepository repository, DiscountProtoService.DiscountProtoServiceClient discountProtoServiceClient) : ICommandHandler<AddItemToBasketCommand, AddItemToBasketCommandResult>
{
    public async Task<AddItemToBasketCommandResult> Handle(AddItemToBasketCommand command, CancellationToken cancellationToken)
    {
        await ApplyItemDiscount(command.CartItem, cancellationToken);
        var shoppingCart = await repository.AddItemToBasketAsync(command.UserName, command.CartItem, cancellationToken);
        return new AddItemToBasketCommandResult(command.UserName, command.CartItem, shoppingCart);
    }

    public async Task ApplyItemDiscount(Models.ShoppingCartItem item, CancellationToken cancellationToken)
    {
        var coupon = await discountProtoServiceClient.GetDiscountAsync(new GetDiscountRequest
            { ProductName = item.ProductName }, cancellationToken: cancellationToken);
        item.Price -= (decimal)coupon.Amount;
    }
}
