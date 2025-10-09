using FluentValidation;

namespace Catalog.API.Features.Products.Commands.ImportProducts;

/// <summary>
/// Validator for <see cref="ImportProductsCommand"/>
/// </summary>
public class ImportProductsCommandValidator : AbstractValidator<ImportProductsCommand>
{
    /// <summary>
    /// Constructor
    /// </summary>
    public ImportProductsCommandValidator()
    {
        RuleFor(x => x.File).NotNull().WithMessage("Le fichier est requis.");
        RuleFor(x => x.File.Length).GreaterThan(0).WithMessage("Le fichier ne peut pas être vide.");
    }
}