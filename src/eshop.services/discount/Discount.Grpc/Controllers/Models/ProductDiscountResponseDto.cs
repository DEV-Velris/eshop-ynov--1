namespace Discount.Grpc.Controllers.Models;

public class ProductDiscountResponseDto
{
    public Guid? ProductId { get; set; }

    public List<DiscountCodeResponseDto> Discounts { get; set; } = [];
}
