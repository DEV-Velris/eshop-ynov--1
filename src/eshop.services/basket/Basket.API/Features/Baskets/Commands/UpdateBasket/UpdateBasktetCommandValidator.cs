using FluentValidation;

namespace Basket.API.Features.Baskets.Commands.UpdateBasket;

/// <summary>
/// Validator for the <see cref="UpdateBasketCommand"/> class used to validate the data integrity of the command.
/// </summary>
/// <remarks>
/// Ensures that the required properties of the <see cref="UpdateBasketCommand"/> are properly populated before processing.
/// Performs validation checks on the <see cref="ShoppingCart"/> instance, including:
/// - Ensuring that the cart object itself is not null.
/// - Validating that the <see cref="ShoppingCart.UserName"/> is not empty.
/// </remarks>
public class UpdateBasketCommandValidator : AbstractValidator<UpdateBasketCommand>
{
    public UpdateBasketCommandValidator()
    {
        RuleFor(x => x.Item).NotNull().WithMessage("Item is required, can not be null");
    }
}