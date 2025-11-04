using FluentValidation;

namespace Catalog.API.Features.Products.Commands.DeleteProduct;

/// <summary>
/// Validates the DeleteProductCommand to ensure that all required properties meet the defined rules and constraints.
/// </summary>
public class DeleteProductCommandValidator : AbstractValidator<DeleteProductCommand>
{
    /// <summary>
    /// Provides validation rules for the DeleteProductCommand.
    /// </summary>
    public DeleteProductCommandValidator()
    {
        RuleFor(product => product.Id).NotEmpty().WithMessage("Id is required")
            .Must(id => Guid.TryParse(id.ToString(), out _)).WithMessage("Id must be a valid GUID");
    }
}