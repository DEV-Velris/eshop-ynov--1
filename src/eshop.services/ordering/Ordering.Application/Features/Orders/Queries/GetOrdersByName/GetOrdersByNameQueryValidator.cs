using FluentValidation;

namespace Ordering.Application.Features.Orders.Queries.GetOrdersByName;

public class GetOrdersByNameQueryValidator : AbstractValidator<GetOrdersByNameQuery>
{
    public GetOrdersByNameQueryValidator()
    {
        RuleFor(x => x.OrderName)
            .NotEmpty()
            .WithMessage("Order name is required")
            .MaximumLength(100)
            .WithMessage("Order name must not exceed 100 characters");
    }
}