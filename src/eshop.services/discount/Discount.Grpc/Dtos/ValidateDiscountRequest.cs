using Discount.Grpc.Models;

namespace Discount.Grpc.Dtos;

public record ValidateDiscountResponse(
    bool IsValid,
    string? Reason,
    CouponDto? Coupon);

public record CouponDto(
    string ProductName,
    string? Description,
    decimal AmountOrPercentage,
    CouponDiscountType DiscountType,
    DateTimeOffset? StartDate,
    DateTimeOffset? EndDate,
    string? ProductCategory);