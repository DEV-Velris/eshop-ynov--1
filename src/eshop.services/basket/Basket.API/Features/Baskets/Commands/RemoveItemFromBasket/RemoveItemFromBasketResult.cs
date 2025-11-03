namespace Basket.API.Features.Baskets.Commands.RemoveItemFromBasket;

/// <summary>
/// Represents the result of executing a command to remove an item from a basket.
/// </summary>
public record RemoveItemFromBasketCommandResult(bool IsSuccess, string UserName);