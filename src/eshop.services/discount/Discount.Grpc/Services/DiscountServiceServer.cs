using System.Globalization;
using Discount.Grpc.Data;
using Discount.Grpc.Models;
using Discount.Grpc.Models.Enums;
using Discount.Grpc.Services.Models;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;

namespace Discount.Grpc.Services;

/// <summary>
///     gRPC implementation exposing the discount management API.
/// </summary>
public class DiscountServiceServer : DiscountProtoService.DiscountProtoServiceBase
{
    private readonly DiscountContext dbContext;
    private readonly DiscountEvaluationService evaluationService;
    private readonly ILogger<DiscountServiceServer> logger;

    public DiscountServiceServer(
        DiscountContext dbContext,
        DiscountEvaluationService evaluationService,
        ILogger<DiscountServiceServer> logger)
    {
        this.dbContext = dbContext;
        this.evaluationService = evaluationService;
        this.logger = logger;
    }

    public override async Task<CouponModel> GetDiscount(GetDiscountRequest request, ServerCallContext context)
    {
        logger.LogInformation("Fetching discount for product {ProductName} / {ProductId} or code {Code}", request.ProductName, request.ProductId, request.Code);

        var coupon = await FindCouponAsync(request, context.CancellationToken).ConfigureAwait(false)
                     ?? throw new RpcException(new Status(StatusCode.NotFound, "Discount not found"));

        coupon.RefreshStatus(DateTimeOffset.UtcNow);
        coupon.EnsureStackingThreshold();

        return MapToModel(coupon);
    }

    public override async Task<CouponModel> CreateDiscount(CreateDiscountRequest request, ServerCallContext context)
    {
        if (request.Coupon is null)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Coupon is required"));
        }

        var coupon = new Coupon
        {
            Tiers = []
        };

        ApplyModelToEntity(request.Coupon, coupon);
        coupon.RefreshStatus(DateTimeOffset.UtcNow);

        await dbContext.Coupons.AddAsync(coupon, context.CancellationToken).ConfigureAwait(false);
        await dbContext.SaveChangesAsync(context.CancellationToken).ConfigureAwait(false);

        logger.LogInformation("Discount {Id} created", coupon.Id);

        return MapToModel(coupon);
    }

    public override async Task<CouponModel> UpdateDiscount(UpdateDiscountRequest request, ServerCallContext context)
    {
        if (request.Coupon is null)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Coupon is required"));
        }

        var coupon = await dbContext.Coupons.Include(c => c.Tiers)
            .FirstOrDefaultAsync(c => c.Id == request.Coupon.Id
                                      || (!string.IsNullOrWhiteSpace(request.Coupon.ProductName) && c.ProductName == request.Coupon.ProductName)
                                      || (!string.IsNullOrWhiteSpace(request.Coupon.Code) && c.Code == request.Coupon.Code),
                context.CancellationToken)
            .ConfigureAwait(false);

        if (coupon is null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Discount not found"));
        }

        ApplyModelToEntity(request.Coupon, coupon);
        coupon.RefreshStatus(DateTimeOffset.UtcNow);

        await dbContext.SaveChangesAsync(context.CancellationToken).ConfigureAwait(false);

        logger.LogInformation("Discount {Id} updated", coupon.Id);

        return MapToModel(coupon);
    }

    public override async Task<DeleteDiscountResponse> DeleteDiscount(DeleteDiscountRequest request, ServerCallContext context)
    {
        if (request.Coupon is null)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Coupon is required"));
        }

        var coupon = await dbContext.Coupons.Include(c => c.Tiers)
            .FirstOrDefaultAsync(c => c.Id == request.Coupon.Id
                                      || (!string.IsNullOrWhiteSpace(request.Coupon.ProductName) && c.ProductName == request.Coupon.ProductName)
                                      || (!string.IsNullOrWhiteSpace(request.Coupon.Code) && c.Code == request.Coupon.Code),
                context.CancellationToken)
            .ConfigureAwait(false);

        if (coupon is null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Discount not found"));
        }

        dbContext.Coupons.Remove(coupon);
        await dbContext.SaveChangesAsync(context.CancellationToken).ConfigureAwait(false);

        logger.LogInformation("Discount {Id} deleted", coupon.Id);

        return new DeleteDiscountResponse { Success = true };
    }

    public override async Task<ApplyDiscountResponse> ApplyDiscounts(ApplyDiscountRequest request, ServerCallContext context)
    {
        if (request.Cart is null)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Cart payload is required"));
        }

        var cartContext = MapToCartContext(request.Cart);
        var result = await evaluationService.ApplyDiscountsAsync(cartContext, context.CancellationToken).ConfigureAwait(false);

        return MapToApplyDiscountResponse(result);
    }

    public override async Task<ValidateDiscountResponse> ValidateDiscount(ValidateDiscountRequest request, ServerCallContext context)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Code is required"));
        }

        var (isValid, coupon, reason) = await evaluationService.ValidateDiscountAsync(request.Code, (decimal)request.CartTotal, context.CancellationToken)
            .ConfigureAwait(false);

        var response = new ValidateDiscountResponse
        {
            IsValid = isValid,
            Reason = reason ?? string.Empty
        };

        if (coupon is not null)
        {
            response.Coupon = MapToModel(coupon);
        }

        return response;
    }

    public override async Task<GetProductDiscountsResponse> GetProductDiscounts(GetProductDiscountsRequest request, ServerCallContext context)
    {
        var categories = request.Categories.ToList();
        Guid? productId = Guid.TryParse(request.ProductId, out var parsed) ? parsed : null;

        var discounts = await evaluationService.GetProductDiscountsAsync(productId, categories, context.CancellationToken)
            .ConfigureAwait(false);

        var response = new GetProductDiscountsResponse
        {
            ProductId = request.ProductId ?? string.Empty
        };
        response.Discounts.AddRange(discounts.Select(MapToModel));

        return response;
    }

    public override async Task<ListDiscountsResponse> ListDiscounts(ListDiscountsRequest request, ServerCallContext context)
    {
        var status = MapToStatus(request.Status);
        var (discounts, totalCount) = await evaluationService.ListDiscountsAsync(request.Page, request.PageSize, status, request.Search, context.CancellationToken)
            .ConfigureAwait(false);

        var response = new ListDiscountsResponse
        {
            TotalCount = totalCount
        };
        response.Discounts.AddRange(discounts.Select(MapToModel));

        return response;
    }

    private async Task<Coupon?> FindCouponAsync(GetDiscountRequest request, CancellationToken cancellationToken)
    {
        var query = dbContext.Coupons.Include(c => c.Tiers).AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            return await query.FirstOrDefaultAsync(c => c.Code == request.Code, cancellationToken).ConfigureAwait(false);
        }

        if (!string.IsNullOrWhiteSpace(request.ProductId) && Guid.TryParse(request.ProductId, out var productId))
        {
            return await query.FirstOrDefaultAsync(c => c.ProductId == productId, cancellationToken).ConfigureAwait(false);
        }

        if (!string.IsNullOrWhiteSpace(request.ProductName))
        {
            return await query.FirstOrDefaultAsync(c => c.ProductName == request.ProductName, cancellationToken).ConfigureAwait(false);
        }

        return null;
    }

    private static CartContext MapToCartContext(ShoppingCartModel cart)
    {
        var items = cart.Items
            .Select(item => new CartItemContext(
                Guid.TryParse(item.ProductId, out var parsedId) ? parsedId : null,
                item.ProductName ?? string.Empty,
                item.Categories.ToList(),
                Convert.ToDecimal(item.UnitPrice),
                item.Quantity))
            .ToList();

        return new CartContext(
            cart.UserName ?? string.Empty,
            items,
            string.IsNullOrWhiteSpace(cart.Code) ? null : cart.Code,
            Convert.ToDecimal(cart.ExistingCouponPercentage),
            Convert.ToDecimal(cart.CartTotal));
    }

    private ApplyDiscountResponse MapToApplyDiscountResponse(DiscountComputationResult result)
    {
        var response = new ApplyDiscountResponse
        {
            CartDiscount = (double)result.CartDiscount,
            FinalTotal = (double)result.FinalTotal
        };

        response.Items.AddRange(result.Items.Select(item =>
        {
            var itemModel = new ItemDiscountModel
            {
                ProductId = item.ProductId?.ToString() ?? string.Empty,
                OriginalUnitPrice = (double)item.OriginalUnitPrice,
                DiscountedUnitPrice = (double)item.DiscountedUnitPrice,
                TotalDiscount = (double)item.TotalDiscount,
                Quantity = item.Quantity
            };
            itemModel.AppliedDiscounts.AddRange(item.AppliedDiscounts.Select(d => new AppliedDiscountModel
            {
                DiscountId = d.DiscountId,
                Code = d.Code ?? string.Empty,
                Description = d.Description ?? string.Empty,
                PercentageApplied = (double)d.PercentageApplied,
                AmountApplied = (double)d.AmountApplied
            }));
            return itemModel;
        }));

        return response;
    }

    private CouponModel MapToModel(Coupon coupon)
    {
        var model = new CouponModel
        {
            Id = coupon.Id,
            ProductId = coupon.ProductId?.ToString() ?? string.Empty,
            ProductName = coupon.ProductName ?? string.Empty,
            Description = coupon.Description ?? string.Empty,
            Percentage = (double)coupon.Percentage,
            FixedAmount = (double)coupon.FixedAmount,
            Code = coupon.Code ?? string.Empty,
            Type = MapToModel(coupon.Type),
            Status = MapToModel(coupon.Status),
            StartDate = coupon.StartDate.ToString("O"),
            EndDate = coupon.EndDate.ToString("O"),
            AllowStacking = coupon.AllowStacking,
            MaxStackPercentage = (double)coupon.MaxStackPercentage,
            MinimumAmount = (double)coupon.MinimumAmount,
            Category = coupon.Category ?? string.Empty,
            AutoApply = coupon.AutoApply,
            IsDisabled = coupon.IsDisabled
        };

        model.Tiers.AddRange(coupon.Tiers
            .OrderBy(t => t.ThresholdAmount)
            .Select(t => new DiscountTierModel
            {
                Id = t.Id,
                ThresholdAmount = (double)t.ThresholdAmount,
                Percentage = (double)t.Percentage,
                FixedAmount = (double)t.FixedAmount
            }));

        return model;
    }

    private void ApplyModelToEntity(CouponModel model, Coupon coupon)
    {
        coupon.ProductName = model.ProductName ?? string.Empty;
        coupon.Description = model.Description ?? string.Empty;
        coupon.Percentage = Convert.ToDecimal(model.Percentage);
        coupon.FixedAmount = Convert.ToDecimal(model.FixedAmount);
        coupon.Code = model.Code ?? string.Empty;
        coupon.Type = MapToEntity(model.Type);
        coupon.Status = MapToStatus(model.Status);
        coupon.StartDate = ParseDate(model.StartDate, coupon.StartDate);
        coupon.EndDate = ParseDate(model.EndDate, coupon.EndDate);
        coupon.AllowStacking = model.AllowStacking;
        coupon.MaxStackPercentage = Convert.ToDecimal(model.MaxStackPercentage);
        coupon.MinimumAmount = Convert.ToDecimal(model.MinimumAmount);
        coupon.Category = model.Category ?? string.Empty;
        coupon.AutoApply = model.AutoApply;
        coupon.IsDisabled = model.IsDisabled;

        if (Guid.TryParse(model.ProductId, out var productId))
        {
            coupon.ProductId = productId;
        }
        else
        {
            coupon.ProductId = null;
        }

        UpdateTiers(model, coupon);
    }

    private void UpdateTiers(CouponModel model, Coupon coupon)
    {
        if (coupon.Tiers is null)
        {
            coupon.Tiers = new List<DiscountTier>();
        }

        if (coupon.Tiers.Count > 0)
        {
            dbContext.DiscountTiers.RemoveRange(coupon.Tiers);
            coupon.Tiers.Clear();
        }

        foreach (var tierModel in model.Tiers)
        {
            coupon.Tiers.Add(new DiscountTier
            {
                ThresholdAmount = Convert.ToDecimal(tierModel.ThresholdAmount),
                Percentage = Convert.ToDecimal(tierModel.Percentage),
                FixedAmount = Convert.ToDecimal(tierModel.FixedAmount)
            });
        }
    }

    private static DateTimeOffset ParseDate(string value, DateTimeOffset fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return fallback;
        }

        return DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsed)
            ? parsed
            : fallback;
    }

    private static DiscountType MapToEntity(DiscountTypeModel model) => model switch
    {
        DiscountTypeModel.DiscountTypeModelPercentagePlusFixed => DiscountType.PercentagePlusFixedAmount,
        _ => DiscountType.Percentage
    };

    private static DiscountTypeModel MapToModel(DiscountType type) => type switch
    {
        DiscountType.PercentagePlusFixedAmount => DiscountTypeModel.DiscountTypeModelPercentagePlusFixed,
        _ => DiscountTypeModel.DiscountTypeModelPercentage
    };

    private static DiscountStatus MapToStatus(DiscountStatusModel model) => model switch
    {
        DiscountStatusModel.DiscountStatusModelExpired => DiscountStatus.Expired,
        DiscountStatusModel.DiscountStatusModelDisabled => DiscountStatus.Disabled,
        DiscountStatusModel.DiscountStatusModelUpcoming => DiscountStatus.Upcoming,
        _ => DiscountStatus.Active
    };

    private static DiscountStatusModel MapToModel(DiscountStatus status) => status switch
    {
        DiscountStatus.Expired => DiscountStatusModel.DiscountStatusModelExpired,
        DiscountStatus.Disabled => DiscountStatusModel.DiscountStatusModelDisabled,
        DiscountStatus.Upcoming => DiscountStatusModel.DiscountStatusModelUpcoming,
        _ => DiscountStatusModel.DiscountStatusModelActive
    };
}
