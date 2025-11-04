using Discount.API.Models;

namespace Discount.API.Dtos;

public record CouponDto(
    string ProductName,
    string Description,
    decimal Amount,
    CouponDiscountType Type,
    DateTime? StartsAt,
    DateTime? ExpiresAt,
    string? Category,
    decimal? MaxPercentageCap);