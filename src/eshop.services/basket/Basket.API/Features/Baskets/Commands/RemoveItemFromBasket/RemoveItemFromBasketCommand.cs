using Basket.API.Models;
using BuildingBlocks.CQRS;

namespace Basket.API.Features.Baskets.Commands.RemoveItemFromBasket;

/// <summary>
/// A command to remove an item from a shopping basket, encapsulating the required details within a <see cref="ShoppingCart"/> instance.
/// </summary>
/// <remarks>
/// This command is used to initiate the removal of an item from a basket for a specific user.
/// It implements the <see cref="ICommand{TResponse}"/> interface, where the response type is <see cref="RemoveItemFromBasketCommandResult"/>.
/// </remarks>
/// <param name="UserName">The user name associated with the shopping basket to be updated.</param>
/// <param name="Item">The item to be added or updated in the shopping basket.</param>
/// </param>
public record RemoveItemFromBasketCommand(string UserName, string ProductId, bool IsSuccessful) : ICommand<RemoveItemFromBasketCommandResult>;