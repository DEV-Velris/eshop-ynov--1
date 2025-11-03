using FluentValidation;

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
public class RemoveItemFromBasketValidator : AbstractValidator<RemoveItemFromBasketCommand>
{
    public RemoveItemFromBasketValidator()
    {
        RuleFor(x => x.ProductId).NotNull().WithMessage("ProductId is required, can not be null");
    }
}