using Basket.API.Exceptions;
using Basket.API.Models;
using Marten;
using ShoppingItem.API.Exceptions;

namespace Basket.API.Data.Repositories;

/// <summary>
/// Provides methods to interact with and manage shopping cart data storage.
/// </summary>
public class BasketRepository(IDocumentSession session) : IBasketRepository
{
    /// <summary>
    /// Deletes the shopping cart associated with the specified username.
    /// </summary>
    /// <param name="userName">The username for which the shopping cart needs to be deleted.</param>
    /// <param name="cancellationToken">Optional. A token to cancel the asynchronous operation.</param>
    /// <returns>A boolean indicating whether the deletion was successful.</returns>
    public async Task<bool> DeleteBasketAsync(string userName, CancellationToken cancellationToken = default)
    {
        session.Delete<ShoppingCart>(userName);
        await session.SaveChangesAsync(cancellationToken);
        return true;
    }

    /// <summary>
    /// Retrieves the shopping cart for the specified user by their username.
    /// </summary>
    /// <param name="userName">The username for which the shopping cart needs to be retrieved.</param>
    /// <param name="cancellationToken">Optional. A token to cancel the asynchronous operation.</param>
    /// <returns>The shopping cart associated with the specified username, or null if no such cart exists.</returns>
    /// <exception cref="BasketNotFoundException">Thrown when no shopping cart is found for the specified username.</exception>
    public async Task<ShoppingCart> GetBasketByUserNameAsync(string userName,
        CancellationToken cancellationToken = default)
    {
        var basket = await session.LoadAsync<ShoppingCart>(userName, cancellationToken);
        if (basket is null)
            throw new BasketNotFoundException(userName);

        return basket;
    }

    /// <summary>
    /// Creates a new shopping cart for the specified user.
    /// </summary>
    /// <param name="basket">The shopping cart instance to be created, containing the user's details and items.</param>
    /// <param name="cancellationToken">Optional. A token to cancel the asynchronous operation.</param>
    /// <returns>The created shopping cart instance.</returns>
    public async Task<ShoppingCart> CreateBasketAsync(ShoppingCart basket,
        CancellationToken cancellationToken = default)
    {
        session.Store(basket);
        await session.SaveChangesAsync(cancellationToken);
        return basket;
    }

    /// <summary>
    /// Updates an existing shopping cart for the specified user.
    /// </summary>
    /// <param name="basket">The shopping cart instance to be updated.</param>
    /// <param name="cancellationToken">Optional. A token to cancel the asynchronous operation.</param>
    /// <returns>The updated shopping cart instance.</returns>
    public async Task<ShoppingCart> UpdateBasketAsync(string userName, ShoppingCartItem item, CancellationToken cancellationToken = default)
    {
        var basket = await GetBasketByUserNameAsync(userName, cancellationToken);

        var existingItem = basket.Items.FirstOrDefault(x => x.ProductId == item.ProductId);

        if (existingItem != null)
        {
            existingItem.Quantity += 1;
        }
        else
        {
            throw new ShoppingItemNotFoundException(item.ProductId.ToString());
        }

        session.Store(basket);
        await session.SaveChangesAsync(cancellationToken);
        return basket;
    }

    /// <summary>
    /// Removes an item from the shopping cart for the specified user.
    /// </summary>
    /// <param name="userName">The username whose shopping cart is to be modified.</param>
    /// <param name="productId">The ID of the product to be removed from the cart.</param>
    /// <param name="cancellationToken">Optional. A token to cancel the asynchronous operation.</param>
    /// <returns>The updated shopping cart instance after the item has been removed.</returns>
    /// <exception cref="BasketNotFoundException">Thrown when no shopping cart is found for the specified username.</exception>
    /// <exception cref="ShoppingItemNotFoundException">Thrown when the specified item is not found in the shopping cart.</exception>
    public async Task<ShoppingCart> RemoveItemFromBasketAsync(string userName, string productId, CancellationToken cancellationToken = default)
    {
        var basket = await GetBasketByUserNameAsync(userName, cancellationToken);

        var itemToRemove = basket.Items.FirstOrDefault(x => x.ProductId == Guid.Parse(productId)) ?? throw new ShoppingItemNotFoundException(productId);

        var quantity = itemToRemove.Quantity;
        if (quantity > 1)
        {
            itemToRemove.Quantity -= 1;
        }
        else
        {
            basket.Items = [.. basket.Items.Where(x => x.ProductId != itemToRemove.ProductId)];
        }

        session.Store(basket);
        await session.SaveChangesAsync(cancellationToken);
        return basket;
    }

    /// <summary>
    /// Adds an item to the shopping cart for the specified user.
    /// </summary>
    /// <param name="userName">The username whose shopping cart is to be modified.</param>
    /// <param name="basket">The shopping cart instance containing the item to be added.</param>
    /// <param name="cancellationToken">Optional. A token to cancel the asynchronous operation.</param>
    /// <returns>The updated shopping cart instance after the item has been added.</returns>
    /// <exception cref="BasketNotFoundException">Thrown when no shopping cart is found for the specified username.</exception>
    public async Task<ShoppingCart> AddItemToBasketAsync(string userName, ShoppingCartItem item, CancellationToken cancellationToken = default)
    {
        var existingBasket = await GetBasketByUserNameAsync(userName, cancellationToken) ?? throw new BasketNotFoundException(userName);

        var existingItem = existingBasket.Items.FirstOrDefault(x => x.ProductId == item.ProductId);

        if (existingItem != null)
        {
            existingItem.Quantity += 1;
        }
        else
        {
            existingBasket.Items = [.. existingBasket.Items, item];
        }

        session.Store(existingBasket);
        await session.SaveChangesAsync(cancellationToken);
        return existingBasket;
    }
}