namespace Discount.Grpc.Services.Models;

/// <summary>
///     Represents the discount computation result for a specific cart line.
/// </summary>
/// <param name="ProductId">Product identifier targeted by the discount result.</param>
/// <param name="Quantity">Quantity of items impacted by the discount.</param>
/// <param name="OriginalUnitPrice">Original unit price before any discount.</param>
/// <param name="DiscountedUnitPrice">Unit price after all discounts are applied.</param>
/// <param name="TotalDiscount">Total discount value for the whole line (quantity included).</param>
/// <param name="AppliedDiscounts">List of applied discount details.</param>
public record ItemDiscountResult(
    Guid? ProductId,
    int Quantity,
    decimal OriginalUnitPrice,
    decimal DiscountedUnitPrice,
    decimal TotalDiscount,
    IReadOnlyCollection<AppliedDiscountResult> AppliedDiscounts);
