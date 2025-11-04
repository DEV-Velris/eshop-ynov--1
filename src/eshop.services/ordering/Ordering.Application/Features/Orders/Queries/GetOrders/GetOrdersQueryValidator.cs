using FluentValidation;

namespace Ordering.Application.Features.Orders.Queries.GetOrders;

public class GetOrdersQueryValidator : AbstractValidator<GetOrdersQuery>
{
    public GetOrdersQueryValidator()
    {
        RuleFor(x => x.PaginationRequest.PageIndex)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Page index must be greater than or equal to 0");

        RuleFor(x => x.PaginationRequest.PageSize)
            .GreaterThan(0)
            .WithMessage("Page size must be greater than 0")
            .LessThanOrEqualTo(100)
            .WithMessage("Page size must not exceed 100");
    }
}