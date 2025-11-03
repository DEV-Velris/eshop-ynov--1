using Discount.Grpc.Controllers.Extensions;
using Discount.Grpc.Controllers.Models;
using Discount.Grpc.Models.Enums;
using Discount.Grpc.Services;
using Microsoft.AspNetCore.Mvc;

namespace Discount.Grpc.Controllers;

[ApiController]
[Route("discounts")]
public class DiscountsController(DiscountEvaluationService evaluationService, ILogger<DiscountsController> logger) : ControllerBase
{
    [HttpPost("apply")]
    [ProducesResponseType(typeof(ApplyDiscountResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApplyDiscountResponseDto>> ApplyDiscount([FromBody] ApplyDiscountRequestDto request, CancellationToken cancellationToken)
    {
        var result = await evaluationService.ApplyDiscountsAsync(request.ToCartContext(), cancellationToken).ConfigureAwait(false);
        return Ok(result.ToResponseDto());
    }

    [HttpGet("validate/{code}")]
    [ProducesResponseType(typeof(ValidateDiscountResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ValidateDiscountResponseDto>> ValidateDiscount(string code, [FromQuery] decimal cartTotal, CancellationToken cancellationToken)
    {
        var (isValid, coupon, reason) = await evaluationService.ValidateDiscountAsync(code, cartTotal, cancellationToken).ConfigureAwait(false);

        var response = new ValidateDiscountResponseDto
        {
            IsValid = isValid,
            Reason = reason ?? string.Empty,
            Coupon = coupon?.ToDto()
        };

        return Ok(response);
    }

    [HttpGet("product/{productId}")]
    [ProducesResponseType(typeof(ProductDiscountResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProductDiscountResponseDto>> GetDiscountsForProduct(Guid productId, [FromQuery] string? categories, CancellationToken cancellationToken)
    {
        var categoryList = string.IsNullOrWhiteSpace(categories)
            ? new List<string>()
            : categories.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

        var discounts = await evaluationService.GetProductDiscountsAsync(productId, categoryList, cancellationToken).ConfigureAwait(false);

        var response = new ProductDiscountResponseDto
        {
            ProductId = productId,
            Discounts = discounts.Select(d => d.ToDto()).ToList()
        };

        return Ok(response);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedDiscountCodeResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedDiscountCodeResponseDto>> ListDiscounts([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] DiscountStatus? status = null, [FromQuery] string? search = null, CancellationToken cancellationToken = default)
    {
        var (discounts, totalCount) = await evaluationService.ListDiscountsAsync(page, pageSize, status, search, cancellationToken).ConfigureAwait(false);

        return Ok(new PagedDiscountCodeResponseDto
        {
            TotalCount = totalCount,
            Items = discounts.Select(d => d.ToDto()).ToList()
        });
    }
}
