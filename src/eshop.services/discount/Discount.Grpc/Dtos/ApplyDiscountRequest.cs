namespace Discount.Grpc.Dtos;

public record ApplyDiscountRequest(
    string ProductName,
    decimal Price,
    string? ProductCategory = null);

public record ApplyDiscountResponse(
    decimal OriginalPrice,
    decimal FinalPrice,
    decimal DiscountAmount,
    string? CouponCode,
    string DiscountType);