using Basket.API.Models;

namespace Basket.API.Features.Baskets.Commands.AddItemToBasket;

/// <summary>
/// Represents the result of executing a command to remove an item from a basket.
/// </summary>
public record AddItemToBasketCommandResult(string UserName, ShoppingCartItem CartItem, ShoppingCart ShoppingCart);