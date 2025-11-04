using Discount.Grpc.Data;
using Discount.Grpc.Dtos;
using Discount.Grpc.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Discount.Grpc.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DiscountsController(DiscountContext context) : ControllerBase
{


    /// <summary>
    /// Applique une réduction sur un produit
    /// </summary>
    /// <param name="request">Informations du produit</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Prix avec réduction appliquée</returns>
    [HttpPost("apply")]
    [ProducesResponseType(typeof(ApplyDiscountResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApplyDiscountResponse>> ApplyDiscount(
        [FromBody] ApplyDiscountRequest request,
        CancellationToken cancellationToken)
    {
        var coupon = await context.Coupons
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.ProductName == request.ProductName, cancellationToken);

        if (coupon is null)
        {
            return NotFound(new { message = "Aucune réduction disponible pour ce produit" });
        }

        var now = DateTime.UtcNow;
        var validationResult = ValidateCoupon(coupon, now, request.ProductCategory);

        if (!validationResult.IsValid)
        {
            return BadRequest(new { message = validationResult.Reason });
        }

        var discountAmount = CalculateDiscount(coupon, request.Price);
        var finalPrice = Math.Max(0, request.Price - discountAmount);

        return Ok(new ApplyDiscountResponse(
            request.Price,
            finalPrice,
            discountAmount,
            coupon.ProductName,
            coupon.Type.ToString()));
    }

    /// <summary>
    /// Valide un code de réduction
    /// </summary>
    /// <param name="code">Code promo à valider</param>
    /// <param name="productCategory">Catégorie du produit (optionnel)</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Informations de validité</returns>
    [HttpGet("validate/{code}")]
    [ProducesResponseType(typeof(ValidateDiscountResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ValidateDiscountResponse>> ValidateDiscount(
        string code,
        [FromQuery] string? productCategory = null,
        CancellationToken cancellationToken = default)
    {
        var coupon = await context.Coupons
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Code == code, cancellationToken);

        if (coupon is null)
        {
            return Ok(new ValidateDiscountResponse(false, "Code non trouvé", null));
        }

        var now = DateTime.UtcNow;
        var validationResult = ValidateCoupon(coupon, now, productCategory);

        var couponDto = new CouponDto(
            coupon.ProductName,
            coupon.Description,
            coupon.Amount,
            coupon.Type,
            coupon.StartsAt,
            coupon.ExpiresAt,
            coupon.Category,
            coupon.MaxPercentageCap);

        return Ok(new ValidateDiscountResponse(validationResult.IsValid, validationResult.Reason, couponDto));
    }

    /// <summary>
    /// Récupère les réductions applicables à un produit
    /// </summary>
    /// <param name="productId">Nom/ID du produit</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Liste des réductions disponibles</returns>
    [HttpGet("product/{productId}")]
    [ProducesResponseType(typeof(CouponDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CouponDto>> GetProductDiscount(
        string productId,
        CancellationToken cancellationToken)
    {
        var coupon = await context.Coupons
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.ProductName == productId, cancellationToken);

        if (coupon is null)
        {
            return NotFound(new { message = "Aucune réduction pour ce produit" });
        }

        var dto = new CouponDto(
            coupon.ProductName,
            coupon.Description,
            coupon.Amount,
            coupon.Type,
            coupon.StartsAt,
            coupon.ExpiresAt,
            coupon.Category,
            coupon.MaxPercentageCap);

        return Ok(dto);
    }

    /// <summary>
    /// Récupère tous les coupons actifs
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>Liste des coupons actifs</returns>
    /// <summary>
    /// Récupère tous les coupons actifs
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>Liste des coupons actifs</returns>
    [HttpGet("active")]
    [ProducesResponseType(typeof(IEnumerable<CouponDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CouponDto>>> GetActiveDiscounts(
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var coupons = await context.Coupons
            .AsNoTracking()
            .Where(c => (c.StartsAt == null || c.StartsAt.Value <= now) &&
                        (c.ExpiresAt == null || c.ExpiresAt.Value >= now))
            .ToListAsync(cancellationToken);

        var dtos = coupons.Select(c => new CouponDto(
            c.ProductName,
            c.Description,
            c.Amount,
            c.Type,
            c.StartsAt,
            c.ExpiresAt,
            c.Category,
            c.MaxPercentageCap));

        return Ok(dtos);
    }

    private static (bool IsValid, string? Reason) ValidateCoupon(Coupon coupon, DateTime now, string? productCategory)
    {
        if (coupon.StartsAt.HasValue && now < coupon.StartsAt.Value)
            return (false, "La réduction n'a pas encore commencé");

        if (coupon.ExpiresAt.HasValue && now > coupon.ExpiresAt.Value)
            return (false, "La réduction a expiré");

        if (!string.IsNullOrEmpty(coupon.Category) &&
            !string.IsNullOrEmpty(productCategory) &&
            !string.Equals(coupon.Category, productCategory, StringComparison.OrdinalIgnoreCase))
            return (false, "Catégorie de produit non compatible");

        if (coupon.Type == CouponDiscountType.Amount && coupon.Amount <= 0)
            return (false, "Montant de réduction invalide");

        if (coupon.Type == CouponDiscountType.Percentage && coupon.Amount <= 0)
            return (false, "Pourcentage de réduction invalide");

        return (true, null);
    }

    private static decimal CalculateDiscount(Coupon coupon, decimal price)
    {
        var discount = coupon.Type switch
        {
            CouponDiscountType.Amount => price - coupon.Amount,
            CouponDiscountType.Percentage => price * coupon.Amount / 10000m,
            _ => 0m
        };

        // Appliquer le plafond de pourcentage si défini
        if (coupon.MaxPercentageCap.HasValue)
        {
            var maxDiscount = price * coupon.MaxPercentageCap.Value / 10000m;
            discount = Math.Min(discount, maxDiscount);
        }

        return discount;
    }
}