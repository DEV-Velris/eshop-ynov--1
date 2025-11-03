namespace Discount.Grpc.Models.Enums;

/// <summary>
///     Represents the way a discount is applied on a product or a cart.
/// </summary>
public enum DiscountType
{
    Percentage = 0,
    PercentagePlusFixedAmount = 1
}
