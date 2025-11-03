using Basket.API.Data.Repositories;
using Basket.API.Models;
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
public class AddItemToBasketHandler(IBasketRepository repository, DiscountProtoService.DiscountProtoServiceClient discountClient) : ICommandHandler<AddItemToBasketCommand, AddItemToBasketCommandResult>
{
    public async Task<AddItemToBasketCommandResult> Handle(AddItemToBasketCommand command, CancellationToken cancellationToken)
    {
        if (command.CartItem.OriginalPrice <= 0)
        {
            command.CartItem.OriginalPrice = command.CartItem.Price;
        }

        var shoppingCart = await repository.AddItemToBasketAsync(command.UserName, command.CartItem, cancellationToken);

        await ApplyDiscountsAsync(shoppingCart, discountClient, cancellationToken).ConfigureAwait(false);
        return new AddItemToBasketCommandResult(command.UserName, command.CartItem, shoppingCart);
    }

    private static async Task ApplyDiscountsAsync(ShoppingCart cart, DiscountProtoService.DiscountProtoServiceClient discountClient, CancellationToken cancellationToken)
    {
        foreach (var item in cart.Items)
        {
            if (item.OriginalPrice <= 0)
            {
                item.OriginalPrice = item.Price;
            }
        }

        var cartModel = new ShoppingCartModel
        {
            UserName = cart.UserName,
            CartTotal = (double)cart.Items.Sum(x => x.OriginalPrice * x.Quantity)
        };

        cartModel.Items.AddRange(cart.Items.Select(item => new CartItemModel
        {
            ProductId = item.ProductId.ToString(),
            ProductName = item.ProductName,
            UnitPrice = (double)item.OriginalPrice,
            Quantity = item.Quantity,
            Categories = { item.Categories }
        }));

        var response = await discountClient.ApplyDiscountsAsync(new ApplyDiscountRequest { Cart = cartModel }, cancellationToken: cancellationToken);

        foreach (var itemResult in response.Items)
        {
            if (!Guid.TryParse(itemResult.ProductId, out var productId))
            {
                continue;
            }

            var cartItem = cart.Items.FirstOrDefault(x => x.ProductId == productId);
            if (cartItem is null)
            {
                continue;
            }

            cartItem.Price = (decimal)itemResult.DiscountedUnitPrice;
            cartItem.OriginalPrice = (decimal)itemResult.OriginalUnitPrice;
            cartItem.AppliedDiscounts = itemResult.AppliedDiscounts.Select(d => new ShoppingCartItemDiscount
            {
                DiscountId = d.DiscountId,
                Code = d.Code,
                Description = d.Description,
                PercentageApplied = (decimal)d.PercentageApplied,
                AmountApplied = (decimal)d.AmountApplied
            }).ToList();
        }
    }
}
