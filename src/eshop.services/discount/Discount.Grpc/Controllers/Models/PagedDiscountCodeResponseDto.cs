namespace Discount.Grpc.Controllers.Models;

public class PagedDiscountCodeResponseDto
{
    public int TotalCount { get; set; }

    public List<DiscountCodeResponseDto> Items { get; set; } = [];
}
