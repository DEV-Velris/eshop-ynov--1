using System.ComponentModel.DataAnnotations;
using Discount.Grpc.Models.Enums;

namespace Discount.Grpc.Models;

/// <summary>
///     Represents a discount configuration stored in the Discount service database.
///     A <see cref="Coupon"/> can target a specific product, a category or the whole basket and
///     can expose several rules such as percentage or percentage plus fixed amount promotions.
/// </summary>
public class Coupon
{
    private const decimal DefaultMaxStackPercentage = 0.30m;

    public int Id { get; set; }

    /// <summary>
    ///     Gets or sets the optional identifier of the product the discount applies to. When null the discount
    ///     is considered either category wide or global.
    /// </summary>
    public Guid? ProductId { get; set; }

    /// <summary>
    ///     Gets or sets the human readable product name or target description. This property is kept for backward
    ///     compatibility with the existing basket implementation that uses product names as identifiers.
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the description of the discount.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the percentage applied by the discount. The value is expressed as a fraction (for instance
    ///     <c>0.10</c> for 10 %).
    /// </summary>
    [Range(0, 1)]
    public decimal Percentage { get; set; }

    /// <summary>
    ///     Gets or sets an optional fixed amount reduction that is applied in addition to the percentage part.
    /// </summary>
    [Range(0, double.MaxValue)]
    public decimal FixedAmount { get; set; }

    /// <summary>
    ///     Gets or sets the optional code that must be provided by the customer to activate the discount.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the type of discount (percentage only or percentage plus a fixed amount).
    /// </summary>
    public DiscountType Type { get; set; } = DiscountType.Percentage;

    /// <summary>
    ///     Gets or sets the discount status. The status is updated automatically based on the start/end dates and the
    ///     disabled flag.
    /// </summary>
    public DiscountStatus Status { get; private set; } = DiscountStatus.Upcoming;

    /// <summary>
    ///     Gets or sets the start date of the discount validity period (UTC).
    /// </summary>
    public DateTimeOffset StartDate { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    ///     Gets or sets the end date of the discount validity period (UTC).
    /// </summary>
    public DateTimeOffset EndDate { get; set; } = DateTimeOffset.UtcNow.AddDays(30);

    /// <summary>
    ///     Gets or sets a value indicating whether the discount can be stacked with other discounts.
    /// </summary>
    public bool AllowStacking { get; set; }

    /// <summary>
    ///     Gets or sets the maximum stackable percentage allowed for the discount. The value is capped to 30 % in the
    ///     business logic.
    /// </summary>
    public decimal MaxStackPercentage { get; set; } = DefaultMaxStackPercentage;

    /// <summary>
    ///     Gets or sets the minimum order amount required to apply the discount.
    /// </summary>
    [Range(0, double.MaxValue)]
    public decimal MinimumAmount { get; set; }

    /// <summary>
    ///     Gets or sets the category targeted by the discount. When empty the discount is either product specific or global.
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets a value indicating whether the discount should be applied automatically without requiring a code.
    /// </summary>
    public bool AutoApply { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the discount has been manually disabled.
    /// </summary>
    public bool IsDisabled { get; set; }

    /// <summary>
    ///     Gets or sets the tiered rules associated with the discount.
    /// </summary>
    public ICollection<DiscountTier> Tiers { get; set; } = new List<DiscountTier>();

    /// <summary>
    ///     Updates the <see cref="Status"/> property based on the current UTC date.
    /// </summary>
    public void RefreshStatus(DateTimeOffset currentUtcDate)
    {
        if (IsDisabled)
        {
            Status = DiscountStatus.Disabled;
            return;
        }

        if (EndDate < currentUtcDate)
        {
            Status = DiscountStatus.Expired;
            return;
        }

        if (StartDate > currentUtcDate)
        {
            Status = DiscountStatus.Upcoming;
            return;
        }

        Status = DiscountStatus.Active;
    }

    /// <summary>
    ///     Determines whether the discount is currently active.
    /// </summary>
    public bool IsActive(DateTimeOffset now)
    {
        RefreshStatus(now);
        return Status == DiscountStatus.Active;
    }

    /// <summary>
    ///     Determines whether the discount applies to the specified product context.
    /// </summary>
    public bool AppliesTo(Guid? productId, IReadOnlyCollection<string> categories)
    {
        if (ProductId.HasValue && productId.HasValue && ProductId == productId)
        {
            return true;
        }

        if (!string.IsNullOrWhiteSpace(Category) && categories.Contains(Category, StringComparer.OrdinalIgnoreCase))
        {
            return true;
        }

        return !ProductId.HasValue && string.IsNullOrWhiteSpace(Category);
    }

    /// <summary>
    ///     Ensures that the maximum stacking percentage always respects the default value when not explicitly configured.
    /// </summary>
    public void EnsureStackingThreshold()
    {
        if (MaxStackPercentage <= 0 || MaxStackPercentage > DefaultMaxStackPercentage)
        {
            MaxStackPercentage = DefaultMaxStackPercentage;
        }
    }
}