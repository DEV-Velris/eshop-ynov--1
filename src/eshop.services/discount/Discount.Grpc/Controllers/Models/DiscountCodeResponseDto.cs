using Discount.Grpc.Models.Enums;

namespace Discount.Grpc.Controllers.Models;

public class DiscountCodeResponseDto
{
    public int Id { get; set; }

    public Guid? ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Percentage { get; set; }

    public decimal FixedAmount { get; set; }

    public string Code { get; set; } = string.Empty;

    public DiscountType Type { get; set; }

    public DiscountStatus Status { get; set; }

    public DateTimeOffset StartDate { get; set; }

    public DateTimeOffset EndDate { get; set; }

    public bool AllowStacking { get; set; }

    public decimal MaxStackPercentage { get; set; }

    public decimal MinimumAmount { get; set; }

    public string Category { get; set; } = string.Empty;

    public bool AutoApply { get; set; }

    public bool IsDisabled { get; set; }

    public List<DiscountTierDto> Tiers { get; set; } = [];
}
