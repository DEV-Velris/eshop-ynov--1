using Basket.API.Models;
using BuildingBlocks.CQRS;

namespace Basket.API.Features.Baskets.Commands.AddItemToBasket;

/// <summary>
/// Represents the result of executing a command to add an item to a basket.
/// </summary>
public record AddItemToBasketCommand(string UserName, ShoppingCartItem CartItem) : ICommand<AddItemToBasketCommandResult>;