using FluentValidation;

namespace Ordering.Application.Features.Orders.Commands.UpdateOrderStatus;

/// <summary>
/// Validator for the UpdateOrderStatusCommand.
/// Ensures that the command contains valid data for updating an order's status.
/// </summary>
public class UpdateOrderStatusCommandValidator : AbstractValidator<UpdateOrderStatusCommand>
{
    public UpdateOrderStatusCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("OrderId is required");
        
        RuleFor(x => x.OrderStatus)
            .IsInEnum().WithMessage("OrderStatus must be a valid enum value");
    }
}