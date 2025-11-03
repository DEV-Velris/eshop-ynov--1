namespace Discount.Grpc.Controllers.Models;

public class ApplyDiscountRequestDto
{
    public string UserName { get; set; } = string.Empty;

    public string? Code { get; set; }

    public decimal ExistingCouponPercentage { get; set; }

    public decimal CartTotal { get; set; }

    public List<CartItemDto> Items { get; set; } = [];
}
