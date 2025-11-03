namespace Discount.Grpc.Services.Models;

/// <summary>
///     Represents the cart information sent by the consumer in order to compute discounts.
/// </summary>
/// <param name="UserName">The customer user name associated with the cart.</param>
/// <param name="Items">The list of items contained in the cart.</param>
/// <param name="Code">Optional code provided by the customer.</param>
/// <param name="ExistingCouponPercentage">Percentage already granted by an external coupon (0.1 = 10 %).</param>
/// <param name="CartTotal">Total amount of the cart before applying the discounts handled by this service.</param>
public record CartContext(
    string UserName,
    IReadOnlyCollection<CartItemContext> Items,
    string? Code,
    decimal ExistingCouponPercentage,
    decimal CartTotal);
