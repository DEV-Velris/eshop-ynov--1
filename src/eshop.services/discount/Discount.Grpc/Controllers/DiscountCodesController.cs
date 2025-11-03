using Discount.Grpc.Controllers.Extensions;
using Discount.Grpc.Controllers.Models;
using Discount.Grpc.Data;
using Discount.Grpc.Models;
using Discount.Grpc.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Discount.Grpc.Controllers;

[ApiController]
[Route("discount-codes")]
public class DiscountCodesController(DiscountContext dbContext, ILogger<DiscountCodesController> logger) : ControllerBase
{
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(DiscountCodeResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DiscountCodeResponseDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var coupon = await dbContext.Coupons.Include(c => c.Tiers)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
            .ConfigureAwait(false);

        if (coupon is null)
        {
            return NotFound();
        }

        coupon.RefreshStatus(DateTimeOffset.UtcNow);
        coupon.EnsureStackingThreshold();

        return Ok(coupon.ToDto());
    }

    [HttpPost]
    [ProducesResponseType(typeof(DiscountCodeResponseDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<DiscountCodeResponseDto>> Create([FromBody] DiscountCodeRequestDto request, CancellationToken cancellationToken)
    {
        var coupon = new Coupon
        {
            Tiers = []
        };

        request.ApplyToEntity(coupon);
        coupon.RefreshStatus(DateTimeOffset.UtcNow);

        await dbContext.Coupons.AddAsync(coupon, cancellationToken).ConfigureAwait(false);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return CreatedAtAction(nameof(GetById), new { id = coupon.Id }, coupon.ToDto());
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(DiscountCodeResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DiscountCodeResponseDto>> Update(int id, [FromBody] DiscountCodeRequestDto request, CancellationToken cancellationToken)
    {
        var coupon = await dbContext.Coupons.Include(c => c.Tiers)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
            .ConfigureAwait(false);

        if (coupon is null)
        {
            return NotFound();
        }

        request.ApplyToEntity(coupon);
        coupon.RefreshStatus(DateTimeOffset.UtcNow);

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Ok(coupon.ToDto());
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Disable(int id, CancellationToken cancellationToken)
    {
        var coupon = await dbContext.Coupons.FirstOrDefaultAsync(c => c.Id == id, cancellationToken).ConfigureAwait(false);

        if (coupon is null)
        {
            return NotFound();
        }

        coupon.IsDisabled = true;
        coupon.Status = DiscountStatus.Disabled;

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return NoContent();
    }
}
