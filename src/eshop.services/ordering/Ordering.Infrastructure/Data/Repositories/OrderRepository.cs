using BuildingBlocks.Pagination;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ordering.Application.Features.Orders.Data;
using Ordering.Application.Features.Orders.Dtos;
using Ordering.Domain.Models;
using Ordering.Domain.ValueObjects;
using Ordering.Domain.ValueObjects.Types;
using Ordering.Infrastructure.Data;
using Ordering.Application.Extensions;

namespace Ordering.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for order data access operations using Entity Framework.
/// </summary>
public class OrderRepository(OrderingDbContext dbContext, ILogger<OrderRepository> logger) : IOrderRepository
{
    public async Task<PaginatedResult<OrderDto>> GetOrdersAsync(PaginationRequest paginationRequest, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Fetching orders with pagination: PageIndex={PageIndex}, PageSize={PageSize}", 
            paginationRequest.PageIndex, paginationRequest.PageSize);

        var totalCount = await dbContext.Orders.CountAsync(cancellationToken);
        
        var orders = await dbContext.Orders
            .Include(o => o.OrderItems)
            .OrderByDescending(o => o.CreatedAt)
            .Skip(paginationRequest.PageIndex * paginationRequest.PageSize)
            .Take(paginationRequest.PageSize)
            .ToListAsync(cancellationToken);

        logger.LogInformation("Fetched {Count} orders out of {TotalCount}", orders.Count, totalCount);

        var orderDtos = orders.ToOrderDtoList();

        return new PaginatedResult<OrderDto>(
            paginationRequest.PageIndex,
            paginationRequest.PageSize,
            totalCount,
            orderDtos);
    }

    public async Task<IEnumerable<OrderDto>> GetOrdersByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Fetching orders for customer {CustomerId}", customerId);

        var orders = await dbContext.Orders
            .Include(o => o.OrderItems)
            .Where(o => o.CustomerId == CustomerId.Of(customerId))
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);

        logger.LogInformation("Found {Count} orders for customer {CustomerId}", orders.Count, customerId);
        
        return orders.ToOrderDtoList();
    }

    public async Task<IEnumerable<OrderDto>> GetOrdersByNameAsync(string orderName, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Fetching orders with name containing '{OrderName}'", orderName);

        var orders = await dbContext.Orders
            .Include(o => o.OrderItems)
            .Where(o => o.OrderName.Value.Contains(orderName))
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);

        logger.LogInformation("Found {Count} orders with name containing '{OrderName}'", orders.Count, orderName);
        
        return orders.ToOrderDtoList();
    }

    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Fetching order with ID {OrderId}", id);

        var order = await dbContext.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == OrderId.Of(id), cancellationToken);

        if (order != null)
            logger.LogInformation("Order {OrderId} found", id);
        else
            logger.LogWarning("Order {OrderId} not found", id);

        return order;
    }

    public async Task<Order> CreateAsync(Order order, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating new order {OrderId} for customer {CustomerId}", 
            order.Id.Value, order.CustomerId.Value);

        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync(cancellationToken);
        
        logger.LogInformation("Order {OrderId} created successfully", order.Id.Value);
        
        return order;
    }

    public async Task<Order> UpdateAsync(Order order, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Updating order {OrderId}", order.Id.Value);

        dbContext.Orders.Update(order);
        await dbContext.SaveChangesAsync(cancellationToken);
        
        logger.LogInformation("Order {OrderId} updated successfully", order.Id.Value);
        
        return order;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Deleting order {OrderId}", id);

        var order = await dbContext.Orders.FirstOrDefaultAsync(o => o.Id == OrderId.Of(id), cancellationToken);
        if (order == null)
        {
            logger.LogWarning("Order {OrderId} not found for deletion", id);
            return false;
        }

        dbContext.Orders.Remove(order);
        await dbContext.SaveChangesAsync(cancellationToken);
        
        logger.LogInformation("Order {OrderId} deleted successfully", id);
        
        return true;
    }
}