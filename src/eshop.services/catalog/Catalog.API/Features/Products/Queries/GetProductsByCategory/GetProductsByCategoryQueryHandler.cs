using BuildingBlocks.CQRS;
using Catalog.API.Models;
using Discount.Grpc;
using Marten;
using Microsoft.Extensions.Logging;

namespace Catalog.API.Features.Products.Queries.GetProductsByCategory;

/// <summary>
/// Handles the execution of the <see cref="GetProductsByCategoryQuery"/> and retrieves the corresponding
/// </summary>
/// <param name="documentSession">The document session</param>
public class GetProductsByCategoryQueryHandler(IDocumentSession documentSession, DiscountProtoService.DiscountProtoServiceClient discountClient, ILogger<GetProductsByCategoryQueryHandler> logger)
    : IQueryHandler<GetProductsByCategoryQuery, GetProductsByCategoryQueryResult>
{
    /// <summary>
    /// Handles the execution of the GetProductsByCategoryQuery and retrieves the associated product data.
    /// </summary>
    /// <param name="request">The query containing the category name</param>
    /// <param name="cancellationToken">A token monitor for cancellation requests</param>
    /// <returns>A task that represents the asynchronous operation, containing the result of the query, including the product data</returns>
    public async Task<GetProductsByCategoryQueryResult> Handle(GetProductsByCategoryQuery request,
        CancellationToken cancellationToken)
    {
        var products = await documentSession.Query<Product>()
            .Where(p => p.Categories.Any(c => c.Equals(request.Category, StringComparison.InvariantCultureIgnoreCase)))
            .ToListAsync(cancellationToken);

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

        logger.LogInformation("Found {Count} products in category {Category}", products.Count, request.Category);
        return new GetProductsByCategoryQueryResult(products);
    }
}