namespace Basket.API.Features.Baskets.Commands.UpdateBasket;

/// <summary>
/// Represents the result of executing a command to update a basket.
/// </summary>
public record UpdateBasketCommandResult(bool IsSuccess, string UserName);