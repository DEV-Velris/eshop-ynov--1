using Discount.Grpc.Controllers.Models;
using Discount.Grpc.Models;
using Discount.Grpc.Models.Enums;
using Discount.Grpc.Services.Models;

namespace Discount.Grpc.Controllers.Extensions;

public static class DiscountMappingExtensions
{
    public static CartContext ToCartContext(this ApplyDiscountRequestDto request)
    {
        var items = request.Items
            .Select(item => new CartItemContext(
                item.ProductId == Guid.Empty ? null : item.ProductId,
                item.ProductName,
                item.Categories,
                item.UnitPrice,
                item.Quantity))
            .ToList();

        return new CartContext(
            request.UserName,
            items,
            request.Code,
            request.ExistingCouponPercentage,
            request.CartTotal);
    }

    public static ApplyDiscountResponseDto ToResponseDto(this DiscountComputationResult result)
    {
        return new ApplyDiscountResponseDto
        {
            CartDiscount = result.CartDiscount,
            FinalTotal = result.FinalTotal,
            Items = result.Items.Select(item => new ItemDiscountDto
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                OriginalUnitPrice = item.OriginalUnitPrice,
                DiscountedUnitPrice = item.DiscountedUnitPrice,
                TotalDiscount = item.TotalDiscount,
                AppliedDiscounts = item.AppliedDiscounts.Select(d => new AppliedDiscountDto
                {
                    DiscountId = d.DiscountId,
                    Code = d.Code,
                    Description = d.Description,
                    PercentageApplied = d.PercentageApplied,
                    AmountApplied = d.AmountApplied
                }).ToList()
            }).ToList()
        };
    }

    public static DiscountCodeResponseDto ToDto(this Coupon coupon)
    {
        return new DiscountCodeResponseDto
        {
            Id = coupon.Id,
            ProductId = coupon.ProductId,
            ProductName = coupon.ProductName,
            Description = coupon.Description,
            Percentage = coupon.Percentage,
            FixedAmount = coupon.FixedAmount,
            Code = coupon.Code,
            Type = coupon.Type,
            Status = coupon.Status,
            StartDate = coupon.StartDate,
            EndDate = coupon.EndDate,
            AllowStacking = coupon.AllowStacking,
            MaxStackPercentage = coupon.MaxStackPercentage,
            MinimumAmount = coupon.MinimumAmount,
            Category = coupon.Category,
            AutoApply = coupon.AutoApply,
            IsDisabled = coupon.IsDisabled,
            Tiers = coupon.Tiers.Select(t => new DiscountTierDto
            {
                ThresholdAmount = t.ThresholdAmount,
                Percentage = t.Percentage,
                FixedAmount = t.FixedAmount
            }).ToList()
        };
    }

    public static void ApplyToEntity(this DiscountCodeRequestDto request, Coupon coupon)
    {
        coupon.ProductId = request.ProductId;
        coupon.ProductName = request.ProductName;
        coupon.Description = request.Description;
        coupon.Percentage = request.Percentage;
        coupon.FixedAmount = request.FixedAmount;
        coupon.Code = request.Code;
        coupon.Type = request.Type;
        coupon.Status = request.Status;
        coupon.StartDate = request.StartDate;
        coupon.EndDate = request.EndDate;
        coupon.AllowStacking = request.AllowStacking;
        coupon.MaxStackPercentage = request.MaxStackPercentage;
        coupon.MinimumAmount = request.MinimumAmount;
        coupon.Category = request.Category;
        coupon.AutoApply = request.AutoApply;
        coupon.IsDisabled = request.IsDisabled;

        coupon.Tiers ??= new List<DiscountTier>();
        coupon.Tiers.Clear();
        foreach (var tier in request.Tiers)
        {
            coupon.Tiers.Add(new DiscountTier
            {
                ThresholdAmount = tier.ThresholdAmount,
                Percentage = tier.Percentage,
                FixedAmount = request.Type == DiscountType.Percentage ? 0m : tier.FixedAmount
            });
        }
    }
}
