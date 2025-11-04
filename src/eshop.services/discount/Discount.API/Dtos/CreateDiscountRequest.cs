using Discount.API.Models;

namespace Discount.API.Dtos;

public record CreateDiscountRequestApi(
    string ProductName,
    string Code,
    string Description,
    int AmountOrPercentage,
    CouponDiscountType DiscountType,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    string? ProductCategory = null,
    int? MaxPercentageCap = null);