namespace Discount.Grpc.Services.Models;

/// <summary>
///     Represents the result of applying a single discount on a cart line.
/// </summary>
/// <param name="DiscountId">The discount identifier.</param>
/// <param name="Code">The discount code if any.</param>
/// <param name="Description">A human readable description.</param>
/// <param name="PercentageApplied">Percentage applied to the line (0.1 = 10 %).</param>
/// <param name="AmountApplied">Final monetary value discounted for the line.</param>
public record AppliedDiscountResult(
    int DiscountId,
    string Code,
    string Description,
    decimal PercentageApplied,
    decimal AmountApplied);
