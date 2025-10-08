using FluentValidation;

namespace Catalog.API.Features.Products.Commands.DeleteProduct;

/// <summary>
///     Validates the DeleteProductCommand to ensure that the product ID is provided and valid.
/// </summary>
public class DeleteProductCommandValidator : AbstractValidator<DeleteProductCommand>
{
    /// <summary>
    ///     Provides validation rules for the DeleteProductCommand.
    /// </summary>
    public DeleteProductCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty().WithMessage("L'ID du produit est requis");
    }
}