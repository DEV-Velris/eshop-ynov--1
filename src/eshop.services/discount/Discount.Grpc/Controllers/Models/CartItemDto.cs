namespace Discount.Grpc.Controllers.Models;

public class CartItemDto
{
    public Guid ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public List<string> Categories { get; set; } = [];

    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }
}
