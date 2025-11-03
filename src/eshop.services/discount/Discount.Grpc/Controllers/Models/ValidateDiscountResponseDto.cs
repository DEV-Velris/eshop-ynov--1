namespace Discount.Grpc.Controllers.Models;

public class ValidateDiscountResponseDto
{
    public bool IsValid { get; set; }

    public DiscountCodeResponseDto? Coupon { get; set; }

    public string Reason { get; set; } = string.Empty;
}
