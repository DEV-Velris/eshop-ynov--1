namespace Discount.Grpc.Services.Models;

/// <summary>
///     Represents the aggregated result of a discount computation.
/// </summary>
/// <param name="Items">The per-item discount results.</param>
/// <param name="CartDiscount">Total cart discount amount.</param>
/// <param name="FinalTotal">Final cart total after discounts.</param>
public record DiscountComputationResult(
    IReadOnlyCollection<ItemDiscountResult> Items,
    decimal CartDiscount,
    decimal FinalTotal);
