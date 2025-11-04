using Discount.API.Data;
using Discount.API.Dtos;
using Discount.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Discount.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DiscountsController(DiscountContext context) : ControllerBase
{
    /// <summary>
    /// Crée un nouveau coupon de réduction
    /// </summary>
    /// <param name="request">Informations du coupon à créer</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Le coupon créé</returns>
    [HttpPost]
    [ProducesResponseType(typeof(CouponDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CouponDto>> CreateDiscount(
        [FromBody] CreateDiscountRequestApi request,
        CancellationToken cancellationToken)
    {
        var existingCoupon = await context.Coupons
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Code == request.Code, cancellationToken);

        if (existingCoupon is not null)
        {
            return Conflict(new { message = "Un coupon existe déjà pour ce produit" });
        }

        if (request is { StartDate: not null, EndDate: not null } &&
            request.StartDate.Value >= request.EndDate.Value)
        {
            return BadRequest(new { message = "La date de début doit être antérieure à la date de fin" });
        }

        if (request is { DiscountType: CouponDiscountType.Amount, AmountOrPercentage: <= 0 })
        {
            return BadRequest(new
                { message = "Le montant doit être supérieur à 0 pour une réduction en montant fixe" });
        }

        if (request is { DiscountType: CouponDiscountType.Percentage, AmountOrPercentage: <= 0 or > 10000 })
        {
            return BadRequest(new { message = "Le pourcentage doit être entre 0.01% et 100%" });
        }

        if (request.MaxPercentageCap.HasValue &&
            (request.MaxPercentageCap.Value <= 0 || request.MaxPercentageCap.Value > 10000))
        {
            return BadRequest(new { message = "Le plafond de pourcentage doit être entre 0.01% et 100%" });
        }

        var coupon = new Coupon
        {
            ProductName = request.ProductName,
            Description = request.Description,
            Code = request.Code,
            Amount = request.AmountOrPercentage,
            Type = request.DiscountType,
            StartsAt = request.StartDate,
            ExpiresAt = request.EndDate,
            Category = request.ProductCategory,
            MaxPercentageCap = request.MaxPercentageCap
        };

        context.Coupons.Add(coupon);
        await context.SaveChangesAsync(cancellationToken);

        var dto = new CouponDto(
            coupon.ProductName,
            coupon.Description,
            coupon.Amount,
            coupon.Type,
            coupon.StartsAt,
            coupon.ExpiresAt,
            coupon.Category,
            coupon.MaxPercentageCap);

        return CreatedAtAction(
            nameof(CreateDiscount),
            new { id = coupon.Id },
            dto);
    }
}