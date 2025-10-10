using Basket.API.Models;
using BuildingBlocks.CQRS;

namespace Basket.API.Features.Baskets.Commands.UpdateBasket;

/// <summary>
/// A command to create a shopping basket, encapsulating the required details within a <see cref="ShoppingCart"/> instance.
/// </summary>
/// <remarks>
/// This command is used to initiate the update of a basket for a specific user, containing one or more items.
/// It implements the <see cref="ICommand{TResponse}"/> interface, where the response type is <see cref="UpdateBasketCommandResult"/>.
/// </remarks>
/// <param name="UserName">The user name associated with the shopping basket to be updated.</param>
/// <param name="Item">The item to be added or updated in the shopping basket.</param>
/// </param>
public record UpdateBasketCommand(string UserName, ShoppingCartItem Item) : ICommand<UpdateBasketCommandResult>;