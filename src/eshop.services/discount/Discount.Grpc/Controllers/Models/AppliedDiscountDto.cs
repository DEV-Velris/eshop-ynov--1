namespace Discount.Grpc.Controllers.Models;

public class AppliedDiscountDto
{
    public int DiscountId { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal PercentageApplied { get; set; }

    public decimal AmountApplied { get; set; }
}
