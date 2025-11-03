using BuildingBlocks.Exceptions;

namespace ShoppingItem.API.Exceptions;

/// <summary>
/// Represents an exception that is thrown when a shopping item associated with a specific user
/// cannot be found in the system. This exception is typically used to indicate that the
/// requested shopping item resource does not exist or is inaccessible.
/// </summary>
public class ShoppingItemNotFoundException : NotFoundException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ShoppingItemNotFoundException"/> class.
    /// </summary>
    /// <param name="itemId">The identifier of the shopping item that was not found.</param>
    public ShoppingItemNotFoundException(string itemId) : base("shopping item", itemId) { }
}