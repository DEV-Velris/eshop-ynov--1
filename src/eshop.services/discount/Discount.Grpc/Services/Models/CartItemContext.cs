namespace Discount.Grpc.Services.Models;

/// <summary>
///     Represents an item inside a cart for the discount computation.
/// </summary>
/// <param name="ProductId">Product identifier if available.</param>
/// <param name="ProductName">Optional product name (backward compatibility).</param>
/// <param name="Categories">List of categories associated with the product.</param>
/// <param name="UnitPrice">Original unit price before discount.</param>
/// <param name="Quantity">Number of units in the cart.</param>
public record CartItemContext(
    Guid? ProductId,
    string ProductName,
    IReadOnlyCollection<string> Categories,
    decimal UnitPrice,
    int Quantity);
