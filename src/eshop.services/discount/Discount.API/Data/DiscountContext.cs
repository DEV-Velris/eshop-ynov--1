using Discount.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Discount.API.Data;

public sealed class DiscountContext(DbContextOptions<DiscountContext> options) : DbContext(options)
{
    public DbSet<Coupon> Coupons { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Coupon>().ToTable("Coupon")
            .HasData(
                new Coupon
                {
                    Id = 1,
                    ProductName = "IPhone X",
                    Description = "IPhone X New",
                    Amount = (decimal)150.0,
                    Code = "IPHONE-X-NEW",
                    Type = CouponDiscountType.Amount,
                    IsActive = true,
                    MaxPercentageCap = null,
                    Category = null,
                    StartsAt = null,
                    ExpiresAt = null
                },
                new Coupon
                {
                    Id = 2,
                    ProductName = "Samsung 10",
                    Description = "Samsung 10 New",
                    Amount = 1000,
                    Code = "SAMSUNG-10-NEW",
                    Type = CouponDiscountType.Percentage,
                    IsActive = true,
                    MaxPercentageCap = null,
                    Category = null,
                    StartsAt = null,
                    ExpiresAt = null
                });
    }
}