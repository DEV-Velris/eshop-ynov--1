using Discount.Grpc.Models;
using Discount.Grpc.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Discount.Grpc.Data;

public sealed class DiscountContext(DbContextOptions<DiscountContext> options) : DbContext(options)
{
    public DbSet<Coupon> Coupons { get; set; }

    public DbSet<DiscountTier> DiscountTiers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Coupon>().ToTable("Coupon");
        modelBuilder.Entity<DiscountTier>().ToTable("DiscountTier");

        modelBuilder.Entity<Coupon>()
            .HasMany(c => c.Tiers)
            .WithOne(t => t.Coupon)
            .HasForeignKey(t => t.CouponId)
            .OnDelete(DeleteBehavior.Cascade);

        var seedReferenceDate = new DateTimeOffset(2024, 01, 01, 0, 0, 0, TimeSpan.Zero);

        modelBuilder.Entity<Coupon>().HasData(
            new
            {
                Id = 1,
                ProductId = (Guid?)Guid.Parse("11111111-1111-1111-1111-111111111111"),
                ProductName = "IPhone 15 Pro",
                Description = "Back to school 10%",
                Percentage = 0.10m,
                FixedAmount = 0m,
                Code = string.Empty,
                Type = DiscountType.Percentage,
                Status = DiscountStatus.Active,
                StartDate = seedReferenceDate,
                EndDate = seedReferenceDate.AddMonths(3),
                AllowStacking = false,
                MaxStackPercentage = 0.30m,
                MinimumAmount = 0m,
                Category = string.Empty,
                AutoApply = true,
                IsDisabled = false
            },
            new
            {
                Id = 2,
                ProductId = (Guid?)null,
                ProductName = string.Empty,
                Description = "Electronics seasonal sale",
                Percentage = 0m,
                FixedAmount = 0m,
                Code = string.Empty,
                Type = DiscountType.Percentage,
                Status = DiscountStatus.Active,
                StartDate = seedReferenceDate.AddDays(-15),
                EndDate = seedReferenceDate.AddMonths(1),
                AllowStacking = true,
                MaxStackPercentage = 0.30m,
                MinimumAmount = 100m,
                Category = "Electronics",
                AutoApply = true,
                IsDisabled = false
            },
            new
            {
                Id = 3,
                ProductId = (Guid?)null,
                ProductName = string.Empty,
                Description = "Summer code 10% + 5€",
                Percentage = 0.10m,
                FixedAmount = 5m,
                Code = "SUMMER2024",
                Type = DiscountType.PercentagePlusFixedAmount,
                Status = DiscountStatus.Active,
                StartDate = seedReferenceDate.AddMonths(-1),
                EndDate = seedReferenceDate.AddMonths(2),
                AllowStacking = true,
                MaxStackPercentage = 0.30m,
                MinimumAmount = 50m,
                Category = string.Empty,
                AutoApply = false,
                IsDisabled = false
            },
            new
            {
                Id = 4,
                ProductId = (Guid?)null,
                ProductName = string.Empty,
                Description = "Future home discount",
                Percentage = 0.15m,
                FixedAmount = 0m,
                Code = string.Empty,
                Type = DiscountType.Percentage,
                Status = DiscountStatus.Upcoming,
                StartDate = seedReferenceDate.AddMonths(1),
                EndDate = seedReferenceDate.AddMonths(2),
                AllowStacking = false,
                MaxStackPercentage = 0.30m,
                MinimumAmount = 0m,
                Category = "Home",
                AutoApply = true,
                IsDisabled = false
            }
        );

        modelBuilder.Entity<DiscountTier>().HasData(
            new
            {
                Id = 1,
                CouponId = 2,
                ThresholdAmount = 100m,
                Percentage = 0.05m,
                FixedAmount = 0m
            },
            new
            {
                Id = 2,
                CouponId = 2,
                ThresholdAmount = 200m,
                Percentage = 0.10m,
                FixedAmount = 0m
            }
        );
    }
}