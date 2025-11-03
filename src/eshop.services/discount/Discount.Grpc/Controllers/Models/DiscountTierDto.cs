namespace Discount.Grpc.Controllers.Models;

public class DiscountTierDto
{
    public decimal ThresholdAmount { get; set; }

    public decimal Percentage { get; set; }

    public decimal FixedAmount { get; set; }
}
