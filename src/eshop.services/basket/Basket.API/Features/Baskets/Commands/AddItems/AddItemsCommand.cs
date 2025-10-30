using Basket.API.Models;

namespace Basket.API.Features.Baskets.Commands.AddItems;

public record AddItemsCommand(string userName, List<ShoppingCartItem> items);