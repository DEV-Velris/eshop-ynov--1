namespace Catalog.API.Models;

public class ProductDiscount
{
    public int Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Percentage { get; set; }

    public decimal FixedAmount { get; set; }

    public bool AllowStacking { get; set; }

    public decimal MaxStackPercentage { get; set; }

    public decimal MinimumAmount { get; set; }

    public string Category { get; set; } = string.Empty;

    public bool AutoApply { get; set; }

    public DateTimeOffset StartDate { get; set; }

    public DateTimeOffset EndDate { get; set; }
}
