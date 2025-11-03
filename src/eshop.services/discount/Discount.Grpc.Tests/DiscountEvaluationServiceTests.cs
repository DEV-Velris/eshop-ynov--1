using Discount.Grpc.Data;
using Discount.Grpc.Models;
using Discount.Grpc.Models.Enums;
using Discount.Grpc.Services;
using Discount.Grpc.Services.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Discount.Grpc.Tests;

public class DiscountEvaluationServiceTests
{
    [Fact]
    public async Task ApplyDiscounts_PercentageOnly_DiscountsItem()
    {
        var productId = Guid.NewGuid();
        await using var context = CreateContext(db =>
        {
            db.Coupons.Add(new Coupon
            {
                ProductId = productId,
                ProductName = "Test Product",
                Description = "10%",
                Percentage = 0.10m,
                FixedAmount = 0m,
                Type = DiscountType.Percentage,
                Status = DiscountStatus.Active,
                StartDate = DateTimeOffset.UtcNow.AddDays(-1),
                EndDate = DateTimeOffset.UtcNow.AddDays(10),
                AllowStacking = false,
                AutoApply = true
            });
        });

        var service = new DiscountEvaluationService(context, NullLogger<DiscountEvaluationService>.Instance);

        var cart = new CartContext("user", new[]
        {
            new CartItemContext(productId, "Test Product", Array.Empty<string>(), 100m, 1)
        }, null, 0m, 100m);

        var result = await service.ApplyDiscountsAsync(cart, CancellationToken.None);

        Assert.Equal(90m, result.FinalTotal);
        Assert.Single(result.Items.First().AppliedDiscounts);
    }

    [Fact]
    public async Task ApplyDiscounts_CodeBasedDiscount_AppliesWhenCodeProvided()
    {
        var productId = Guid.NewGuid();
        await using var context = CreateContext(db =>
        {
            db.Coupons.Add(new Coupon
            {
                ProductId = productId,
                ProductName = "Test",
                Description = "Code",
                Percentage = 0.15m,
                FixedAmount = 5m,
                Code = "SAVE15",
                Type = DiscountType.PercentagePlusFixedAmount,
                Status = DiscountStatus.Active,
                StartDate = DateTimeOffset.UtcNow.AddDays(-1),
                EndDate = DateTimeOffset.UtcNow.AddDays(5),
                AllowStacking = false,
                AutoApply = false
            });
        });

        var service = new DiscountEvaluationService(context, NullLogger<DiscountEvaluationService>.Instance);

        var cart = new CartContext("user", new[]
        {
            new CartItemContext(productId, "Test", Array.Empty<string>(), 100m, 1)
        }, "SAVE15", 0m, 100m);

        var result = await service.ApplyDiscountsAsync(cart, CancellationToken.None);

        Assert.True(result.CartDiscount > 0m);
        Assert.True(result.FinalTotal < 100m);
    }

    [Fact]
    public async Task ApplyDiscounts_StackingRespectMaxPercentage()
    {
        var productId = Guid.NewGuid();
        await using var context = CreateContext(db =>
        {
            db.Coupons.AddRange(
                new Coupon
                {
                    ProductId = productId,
                    ProductName = "Item",
                    Description = "Auto 20%",
                    Percentage = 0.20m,
                    Type = DiscountType.Percentage,
                    Status = DiscountStatus.Active,
                    StartDate = DateTimeOffset.UtcNow.AddDays(-1),
                    EndDate = DateTimeOffset.UtcNow.AddDays(5),
                    AllowStacking = true,
                    AutoApply = true
                },
                new Coupon
                {
                    ProductId = productId,
                    ProductName = "Item",
                    Description = "Code 20%",
                    Percentage = 0.20m,
                    Code = "EXTRA",
                    Type = DiscountType.Percentage,
                    Status = DiscountStatus.Active,
                    StartDate = DateTimeOffset.UtcNow.AddDays(-1),
                    EndDate = DateTimeOffset.UtcNow.AddDays(5),
                    AllowStacking = true,
                    AutoApply = false
                });
        });

        var service = new DiscountEvaluationService(context, NullLogger<DiscountEvaluationService>.Instance);

        var cart = new CartContext("user", new[]
        {
            new CartItemContext(productId, "Item", Array.Empty<string>(), 100m, 1)
        }, "EXTRA", 0m, 100m);

        var result = await service.ApplyDiscountsAsync(cart, CancellationToken.None);

        Assert.Equal(70m, result.FinalTotal); // capped to 30% total discount
    }

    [Fact]
    public async Task ApplyDiscounts_IgnoresExpiredCoupon()
    {
        var productId = Guid.NewGuid();
        await using var context = CreateContext(db =>
        {
            db.Coupons.Add(new Coupon
            {
                ProductId = productId,
                ProductName = "Item",
                Description = "Expired",
                Percentage = 0.50m,
                Type = DiscountType.Percentage,
                Status = DiscountStatus.Active,
                StartDate = DateTimeOffset.UtcNow.AddDays(-10),
                EndDate = DateTimeOffset.UtcNow.AddDays(-1),
                AllowStacking = false,
                AutoApply = true
            });
        });

        var service = new DiscountEvaluationService(context, NullLogger<DiscountEvaluationService>.Instance);

        var cart = new CartContext("user", new[]
        {
            new CartItemContext(productId, "Item", Array.Empty<string>(), 50m, 1)
        }, null, 0m, 50m);

        var result = await service.ApplyDiscountsAsync(cart, CancellationToken.None);

        Assert.Equal(50m, result.FinalTotal);
    }

    [Fact]
    public async Task ValidateDiscount_ReturnsReasonForInvalid()
    {
        await using var context = CreateContext(db =>
        {
            db.Coupons.Add(new Coupon
            {
                Code = "WINTER",
                ProductName = "Any",
                Description = "Winter",
                Percentage = 0.20m,
                Type = DiscountType.Percentage,
                Status = DiscountStatus.Active,
                StartDate = DateTimeOffset.UtcNow.AddDays(-1),
                EndDate = DateTimeOffset.UtcNow.AddDays(10),
                MinimumAmount = 200m,
                AllowStacking = false,
                AutoApply = false
            });
        });

        var service = new DiscountEvaluationService(context, NullLogger<DiscountEvaluationService>.Instance);

        var result = await service.ValidateDiscountAsync("WINTER", 50m, CancellationToken.None);

        Assert.False(result.IsValid);
        Assert.NotNull(result.Reason);
    }

    private static DiscountContext CreateContext(Action<DiscountContext> seeder)
    {
        var options = new DbContextOptionsBuilder<DiscountContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new DiscountContext(options);
        seeder(context);
        context.SaveChanges();
        return context;
    }
}
