namespace Discount.Grpc.Models.Enums;

/// <summary>
///     Represents the lifecycle status of a discount.
/// </summary>
public enum DiscountStatus
{
    Active = 0,
    Expired = 1,
    Disabled = 2,
    Upcoming = 3
}
