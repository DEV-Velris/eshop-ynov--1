using Basket.API.Models;
using FluentValidation;

namespace Basket.API.Features.Baskets.Commands.AddItems;

public class AddItemsCommandValidator : AbstractValidator<AddItemsCommand>
{
    public AddItemsCommandValidator()
    {
        RuleForEach<ShoppingCartItem>(x => x.items)
            .NotNull().WithMessage("L'item ne doit pas être nul.")
            .ChildRules(item =>
            {
                item.RuleFor<string>(i => i.ProductName)
                    .NotEmpty().WithMessage("Le nom de l'item ne doit pas être vide.");
                item.RuleFor<string>(i => i.Color)
                    .NotEmpty().WithMessage("La couleur de l'item ne doit pas être vide.");
                item.RuleFor(i => i.Price)
                    .GreaterThanOrEqualTo(0).WithMessage("Le prix de l'item doit être supérieur ou égal à zéro.");
                item.RuleFor(i => i.Quantity)
                    .GreaterThan(0).WithMessage("La quantité de l'item doit être supérieure à zéro.");
                item.RuleFor(i => i.ProductId)
                    .NotEmpty().WithMessage("L'ID du produit de l'item ne doit pas être vide.");
            });

    }
}