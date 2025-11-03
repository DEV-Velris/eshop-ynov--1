using BuildingBlocks.CQRS;
using Catalog.API.Models;
using Discount.Grpc;
using Microsoft.Extensions.Logging;
using Marten;

namespace Catalog.API.Features.Products.Queries.GetProducts;

/// <summary>
/// Handles the execution of the <see cref="GetProductsQuery"/> and retrieves the corresponding
/// </summary>
/// <param name="documentSession">The document session</param>
public class GetProductsQueryHandler(IDocumentSession documentSession, DiscountProtoService.DiscountProtoServiceClient discountClient, ILogger<GetProductsQueryHandler> logger) : IQueryHandler<GetProductsQuery, GetProductsQueryResult>
{
    /// <summary>
    /// Handles the execution of the GetProductsQuery and retrieves the associated product data.
    /// </summary>
    /// <param name="request">Contains the request parameters</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns></returns>
    public async Task<GetProductsQueryResult> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {

        var query = documentSession.Query<Product>().AsQueryable();

        if (!string.IsNullOrEmpty(request.Category))
        {
            query = query.Where(p => p.Categories.Contains(request.Category));
        }

        if (!string.IsNullOrEmpty(request.Name))
        {
            query = query.Where(p => p.Name.Contains(request.Name, StringComparison.OrdinalIgnoreCase));
        }

        if (request.MinPrice.HasValue)
        {
            query = query.Where(p => p.Price >= request.MinPrice.Value);
        }

        if (request.MaxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= request.MaxPrice.Value);
        }

        var products = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        await EnrichWithDiscountsAsync(products, cancellationToken).ConfigureAwait(false);
        logger.LogInformation("Loaded {Count} products with discounts", products.Count);

        return new GetProductsQueryResult(products);
    }

    private async Task EnrichWithDiscountsAsync(List<Product> products, CancellationToken cancellationToken)
    {
        foreach (var product in products)
        {
            var response = await discountClient.GetProductDiscountsAsync(new GetProductDiscountsRequest
            {
                ProductId = product.Id.ToString(),
                Categories = { product.Categories }
            }, cancellationToken: cancellationToken).ConfigureAwait(false);

            product.ActiveDiscounts = response.Discounts.Select(d => new ProductDiscount
            {
                Id = d.Id,
                Code = d.Code,
                Description = d.Description,
                Percentage = (decimal)d.Percentage,
                FixedAmount = (decimal)d.FixedAmount,
                AllowStacking = d.AllowStacking,
                MaxStackPercentage = (decimal)d.MaxStackPercentage,
                MinimumAmount = (decimal)d.MinimumAmount,
                Category = d.Category,
                AutoApply = d.AutoApply,
                StartDate = DateTimeOffset.TryParse(d.StartDate, out var start) ? start : DateTimeOffset.MinValue,
                EndDate = DateTimeOffset.TryParse(d.EndDate, out var end) ? end : DateTimeOffset.MinValue
            }).ToList();
        }
    }
}