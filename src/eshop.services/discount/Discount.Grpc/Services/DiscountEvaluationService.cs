using Discount.Grpc.Data;
using Discount.Grpc.Models;
using Discount.Grpc.Models.Enums;
using Discount.Grpc.Services.Models;
using Microsoft.EntityFrameworkCore;

namespace Discount.Grpc.Services;

/// <summary>
///     Central service used to evaluate the discount rules. The goal of this service is to keep the computation
///     logic reusable across gRPC endpoints and the HTTP controller layer.
/// </summary>
public class DiscountEvaluationService(DiscountContext dbContext, ILogger<DiscountEvaluationService> logger)
{
    private const decimal GlobalStackingCap = 0.30m;

    /// <summary>
    ///     Applies the discount rules to the provided cart context and returns the computation result.
    /// </summary>
    public async Task<DiscountComputationResult> ApplyDiscountsAsync(CartContext cartContext, CancellationToken cancellationToken)
    {
        logger.LogInformation("Applying discounts for user {User} with {ItemCount} items", cartContext.UserName, cartContext.Items.Count);

        if (cartContext.Items.Count == 0)
        {
            return new DiscountComputationResult([], 0m, cartContext.CartTotal);
        }

        var coupons = await LoadCouponsAsync(cancellationToken).ConfigureAwait(false);
        var now = DateTimeOffset.UtcNow;

        foreach (var coupon in coupons)
        {
            coupon.RefreshStatus(now);
            coupon.EnsureStackingThreshold();
        }

        var activeCoupons = coupons.Where(c => c.Status == DiscountStatus.Active).ToList();

        var cartSubtotal = cartContext.CartTotal > 0
            ? cartContext.CartTotal
            : cartContext.Items.Sum(x => x.UnitPrice * x.Quantity);

        var applicableCoupons = activeCoupons
            .Where(c => IsCouponApplicableForCart(cartContext, cartSubtotal, c))
            .ToList();

        var results = new List<ItemDiscountResult>();
        decimal cartDiscountTotal = 0m;

        foreach (var item in cartContext.Items)
        {
            var itemCoupons = applicableCoupons
                .Where(c => c.AppliesTo(item.ProductId, item.Categories))
                .ToList();

            if (itemCoupons.Count == 0)
            {
                results.Add(new ItemDiscountResult(item.ProductId, item.Quantity, item.UnitPrice, item.UnitPrice, 0m, []));
                continue;
            }

            var appliedDiscountIds = new HashSet<int>();
            var appliedDiscounts = new List<AppliedDiscountResult>();

            var originalUnitPrice = item.UnitPrice;
            var currentUnitPrice = originalUnitPrice;
            var totalPercentage = cartContext.ExistingCouponPercentage;

            foreach (var coupon in itemCoupons)
            {
                if (appliedDiscountIds.Contains(coupon.Id))
                {
                    continue;
                }

                if (!coupon.AllowStacking && appliedDiscounts.Count > 0)
                {
                    continue;
                }

                var (percentage, fixedAmount) = ResolveDiscountValues(coupon, cartSubtotal);

                if (percentage <= 0 && fixedAmount <= 0)
                {
                    continue;
                }

                var allowedAdditionalPercentage = Math.Min(coupon.MaxStackPercentage, GlobalStackingCap) - totalPercentage;
                if (allowedAdditionalPercentage <= 0 && fixedAmount <= 0)
                {
                    continue;
                }

                var effectivePercentage = Math.Min(percentage, Math.Max(allowedAdditionalPercentage, 0m));

                var amountFromPercentage = currentUnitPrice * effectivePercentage;
                var totalAmountApplied = amountFromPercentage + fixedAmount;

                if (totalAmountApplied <= 0)
                {
                    continue;
                }

                if (totalAmountApplied > currentUnitPrice)
                {
                    totalAmountApplied = currentUnitPrice;
                }

                currentUnitPrice -= totalAmountApplied;
                if (currentUnitPrice < 0)
                {
                    currentUnitPrice = 0;
                }

                totalPercentage += effectivePercentage;

                appliedDiscounts.Add(new AppliedDiscountResult(
                    coupon.Id,
                    coupon.Code,
                    coupon.Description,
                    effectivePercentage,
                    totalAmountApplied));

                appliedDiscountIds.Add(coupon.Id);

                if (Math.Abs(currentUnitPrice) < 0.0001m)
                {
                    break;
                }
            }

            var lineDiscount = (originalUnitPrice - currentUnitPrice) * item.Quantity;
            cartDiscountTotal += lineDiscount;

            results.Add(new ItemDiscountResult(
                item.ProductId,
                item.Quantity,
                originalUnitPrice,
                currentUnitPrice,
                lineDiscount,
                appliedDiscounts));
        }

        var finalTotal = Math.Max(0m, cartSubtotal - cartDiscountTotal);

        return new DiscountComputationResult(results, cartDiscountTotal, finalTotal);
    }

    /// <summary>
    ///     Validates a discount code, returning the matched coupon if it is active and the optional reason otherwise.
    /// </summary>
    public async Task<(bool IsValid, Coupon? Coupon, string? Reason)> ValidateDiscountAsync(string code, decimal cartTotal, CancellationToken cancellationToken)
    {
        logger.LogInformation("Validating discount code {Code}", code);

        var coupon = await dbContext.Coupons.Include(c => c.Tiers)
            .FirstOrDefaultAsync(c => c.Code != null && c.Code.ToUpper() == code.ToUpper(), cancellationToken)
            .ConfigureAwait(false);

        if (coupon is null)
        {
            return (false, null, "Code inconnu");
        }

        coupon.RefreshStatus(DateTimeOffset.UtcNow);
        if (coupon.Status != DiscountStatus.Active)
        {
            return (false, coupon, "Le code n'est pas actif");
        }

        if (coupon.MinimumAmount > 0 && cartTotal < coupon.MinimumAmount)
        {
            return (false, coupon, "Montant minimum non atteint");
        }

        return (true, coupon, null);
    }

    /// <summary>
    ///     Returns all active discounts matching the provided product identifier or category list.
    /// </summary>
    public async Task<IReadOnlyCollection<Coupon>> GetProductDiscountsAsync(Guid? productId, IReadOnlyCollection<string> categories, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving discounts for product {ProductId} (categories: {Categories})", productId, string.Join(',', categories));

        var coupons = await LoadCouponsAsync(cancellationToken).ConfigureAwait(false);
        var now = DateTimeOffset.UtcNow;

        var activeCoupons = coupons.Where(c =>
        {
            c.RefreshStatus(now);
            return c.Status == DiscountStatus.Active && c.AppliesTo(productId, categories);
        }).ToList();

        foreach (var coupon in activeCoupons)
        {
            coupon.EnsureStackingThreshold();
        }

        return activeCoupons;
    }

    /// <summary>
    ///     Provides a paginated list of discounts optionally filtered by status and search string.
    /// </summary>
    public async Task<(IReadOnlyCollection<Coupon> Discounts, int TotalCount)> ListDiscountsAsync(int page, int pageSize, DiscountStatus? status, string? search, CancellationToken cancellationToken)
    {
        logger.LogInformation("Listing discounts page {Page} size {PageSize} with status {Status}", page, pageSize, status);

        page = page <= 0 ? 1 : page;
        pageSize = pageSize <= 0 ? 10 : Math.Min(pageSize, 100);

        var query = dbContext.Coupons.Include(c => c.Tiers).AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(c => c.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(c => c.ProductName.Contains(search) || c.Description.Contains(search) || c.Code.Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);

        var discounts = await query
            .OrderByDescending(c => c.StartDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var now = DateTimeOffset.UtcNow;
        foreach (var coupon in discounts)
        {
            coupon.RefreshStatus(now);
            coupon.EnsureStackingThreshold();
        }

        return (discounts, totalCount);
    }

    private async Task<List<Coupon>> LoadCouponsAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Coupons.Include(c => c.Tiers)
            .AsNoTracking()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    private static bool IsCouponApplicableForCart(CartContext cartContext, decimal cartSubtotal, Coupon coupon)
    {
        if (coupon.MinimumAmount > 0 && cartSubtotal < coupon.MinimumAmount)
        {
            return false;
        }

        if (!coupon.AutoApply)
        {
            if (string.IsNullOrWhiteSpace(coupon.Code))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(cartContext.Code) || !coupon.Code.Equals(cartContext.Code, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }
        else if (!string.IsNullOrWhiteSpace(coupon.Code) && !string.IsNullOrWhiteSpace(cartContext.Code))
        {
            if (!coupon.Code.Equals(cartContext.Code, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }

    private static (decimal percentage, decimal fixedAmount) ResolveDiscountValues(Coupon coupon, decimal cartSubtotal)
    {
        decimal percentage = coupon.Percentage;
        decimal fixedAmount = coupon.Type == DiscountType.PercentagePlusFixedAmount ? coupon.FixedAmount : 0m;

        if (coupon.Tiers is { Count: > 0 })
        {
            var tier = coupon.Tiers
                .Where(t => t.ThresholdAmount <= cartSubtotal)
                .OrderByDescending(t => t.ThresholdAmount)
                .FirstOrDefault();

            if (tier is not null)
            {
                percentage = tier.Percentage;
                fixedAmount = coupon.Type == DiscountType.PercentagePlusFixedAmount
                    ? tier.FixedAmount
                    : tier.FixedAmount;

                if (coupon.Type == DiscountType.Percentage)
                {
                    fixedAmount = 0m;
                }
            }
        }

        return (Math.Clamp(percentage, 0m, 1m), Math.Max(0m, fixedAmount));
    }
}
