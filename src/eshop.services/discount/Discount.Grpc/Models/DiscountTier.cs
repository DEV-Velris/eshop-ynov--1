using System.ComponentModel.DataAnnotations;

namespace Discount.Grpc.Models;

/// <summary>
///     Represents a tier associated with a discount. Tiers allow the definition of
///     progressive discount rules depending on the cart total (e.g. -5 % after 100 €, -10 % after 200 €).
/// </summary>
public class DiscountTier
{
    public int Id { get; set; }

    public int CouponId { get; set; }

    public Coupon Coupon { get; set; } = null!;

    /// <summary>
    ///     Gets or sets the minimum cart amount (inclusive) that activates this tier.
    /// </summary>
    [Range(0, double.MaxValue)]
    public decimal ThresholdAmount { get; set; }

    /// <summary>
    ///     Gets or sets the percentage applied by the tier. Value expressed as a fraction (0.05 for 5 %).
    /// </summary>
    [Range(0, 1)]
    public decimal Percentage { get; set; }

    /// <summary>
    ///     Gets or sets the optional fixed amount applied when the tier is reached.
    /// </summary>
    [Range(0, double.MaxValue)]
    public decimal FixedAmount { get; set; }
}
