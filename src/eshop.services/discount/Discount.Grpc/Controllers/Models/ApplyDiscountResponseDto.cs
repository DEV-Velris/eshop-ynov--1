namespace Discount.Grpc.Controllers.Models;

public class ApplyDiscountResponseDto
{
    public decimal CartDiscount { get; set; }

    public decimal FinalTotal { get; set; }

    public List<ItemDiscountDto> Items { get; set; } = [];
}
