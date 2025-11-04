namespace Discount.API.Models;

public enum CouponDiscountType
{
    Amount = 0,
    Percentage = 1
}

public class Coupon
{
    public int Id { get; set; }
    
    public string ProductName { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// If Type is Amount, this is the fixed amount to discount.
    /// <br/>
    /// If Type is Percentage, this is the percentage to discount (e.g., 15 for 15%).
    /// </summary>
    public decimal Amount { get; set; }
    
    /// <summary>
    /// Example: "WINTER2025"
    /// </summary>
    public string Code { get; set; } = string.Empty;
    
    public CouponDiscountType Type { get; set; }
    
    public decimal? MaxPercentageCap { get; set; }
    
    public string? Category { get; set; }
    
    public DateTime? StartsAt { get; set; }
    
    public DateTime? ExpiresAt { get; set; }
    
    public bool IsActive { get; set; }
}