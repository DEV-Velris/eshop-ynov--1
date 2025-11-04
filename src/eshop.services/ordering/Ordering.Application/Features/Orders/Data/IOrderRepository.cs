using BuildingBlocks.Pagination;
using Ordering.Application.Features.Orders.Dtos;
using Ordering.Domain.Models;

namespace Ordering.Application.Features.Orders.Data;

/// <summary>
/// Defines the contract for order data access operations.
/// </summary>
public interface IOrderRepository
{
    /// <summary>
    /// Retrieves all orders with pagination support.
    /// </summary>
    /// <param name="paginationRequest">The pagination parameters.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paginated result containing orders.</returns>
    Task<PaginatedResult<OrderDto>> GetOrdersAsync(PaginationRequest paginationRequest, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves orders associated with a specific customer.
    /// </summary>
    /// <param name="customerId">The unique identifier of the customer.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of orders for the specified customer.</returns>
    Task<IEnumerable<OrderDto>> GetOrdersByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves orders filtered by order name.
    /// </summary>
    /// <param name="orderName">The name of the order to filter by.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of orders matching the specified name.</returns>
    Task<IEnumerable<OrderDto>> GetOrdersByNameAsync(string orderName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves an order by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the order.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The order if found; otherwise null.</returns>
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new order.
    /// </summary>
    /// <param name="order">The order to create.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created order.</returns>
    Task<Order> CreateAsync(Order order, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing order.
    /// </summary>
    /// <param name="order">The order to update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated order.</returns>
    Task<Order> UpdateAsync(Order order, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an order by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the order to delete.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the order was deleted; otherwise false.</returns>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}