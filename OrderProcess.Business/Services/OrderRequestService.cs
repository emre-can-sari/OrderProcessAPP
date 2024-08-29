using Microsoft.EntityFrameworkCore;
using OrderProcess.DataAccess;
using OrderProcess.Entities.Dtos;
using OrderProcess.Entities.Entities;
using OrderProcess.Entities.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderProcess.Business.Services;

public class OrderRequestService
{
    private readonly ApplicationDbContext _context;

    public OrderRequestService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<OrderRequest> CreateOrderRequest(OrderRequestDTO orderRequestDto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            List<OrderItem> orderItems = new List<OrderItem>();

            foreach (var productDto in orderRequestDto.OrderItems)
            {
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.Name == productDto.ProductName);

                if (product == null)
                {
                    product = new Product { Name = productDto.ProductName };
                    _context.Products.Add(product);
                }

                OrderItem orderItem = new OrderItem
                {
                    Product = product,
                    Quantity = productDto.ProductQuantity
                };
                orderItems.Add(orderItem);
                await _context.OrderItems.AddAsync(orderItem);
            }

            OrderRequest orderRequest = new OrderRequest
            {
                OrderItems = orderItems,
                DeliveryTime = orderRequestDto.RequestedDeliveryDay,
                
            };
            await _context.OrderRequests.AddAsync(orderRequest);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return orderRequest;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new Exception("Order request creation failed.", ex);
        }
    }


    public async Task<List<OrderRequest>> GetAllOrderRequests()
    {
        return await _context.OrderRequests
            .Where(or => or.Status == OrderStatusEnum.offerExpected)
            .Include(or => or.OrderItems)
                .ThenInclude(oi => oi.Product)
            .ToListAsync();
    }


    public async Task<OrderRequest> UpdateOrderRequest(int id, OrderRequestDTO orderRequestDto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var orderRequest = await _context.OrderRequests
                .Include(or => or.OrderItems)
                .ThenInclude(oi => oi.Product)
                .SingleOrDefaultAsync(or => or.Id == id);

            if (orderRequest == null)
            {
                throw new Exception("Order request not found.");
            }
            if (orderRequest.Status == OrderStatusEnum.accepted) {
                throw new Exception("Confirmed orders cannot be updated");
            }

            _context.OrderItems.RemoveRange(orderRequest.OrderItems);

            List<OrderItem> updatedOrderItems = new List<OrderItem>();

            foreach (var productDto in orderRequestDto.OrderItems)
            {
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.Name == productDto.ProductName);

                if (product == null)
                {
                    product = new Product { Name = productDto.ProductName };
                    await _context.Products.AddAsync(product);
                }
                else
                {
                    product.Name = productDto.ProductName;
                    _context.Products.Update(product);
                }

                OrderItem orderItem = new OrderItem
                {
                    Product = product,
                    Quantity = productDto.ProductQuantity
                };
                updatedOrderItems.Add(orderItem);
                await _context.OrderItems.AddAsync(orderItem);
            }

            orderRequest.OrderItems = updatedOrderItems;
            orderRequest.DeliveryTime = orderRequestDto.RequestedDeliveryDay;

            _context.OrderRequests.Update(orderRequest);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return orderRequest;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new Exception("Order request update failed.", ex);
        }
    }


    public async Task<bool> UpdateOrderStatus(int orderRequestId, string newStatus)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var orderRequest = await _context.OrderRequests
                .SingleOrDefaultAsync(or => or.Id == orderRequestId);

            if (orderRequest == null)
            {
                throw new Exception("Order request not found.");
            }

            orderRequest.Status = newStatus;
            _context.OrderRequests.Update(orderRequest);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new Exception("Order request status update failed.", ex);
        }
    }

    public async Task<List<OrderRequest>> GetOrderRequestsByStatus(string status)
    {
        return await _context.OrderRequests
            .Where(or => or.Status == status)
            .Include(or => or.OrderItems)
            .ThenInclude(oi => oi.Product)
            .ToListAsync();
    }


}
