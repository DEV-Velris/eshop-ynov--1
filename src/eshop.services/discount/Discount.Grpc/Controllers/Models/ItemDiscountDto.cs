namespace Discount.Grpc.Controllers.Models;

public class ItemDiscountDto
{
    public Guid? ProductId { get; set; }

    public int Quantity { get; set; }

    public decimal OriginalUnitPrice { get; set; }

    public decimal DiscountedUnitPrice { get; set; }

    public decimal TotalDiscount { get; set; }

    public List<AppliedDiscountDto> AppliedDiscounts { get; set; } = [];
}
