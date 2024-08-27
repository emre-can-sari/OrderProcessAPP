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
            Product product = new Product { Name = orderRequestDto.ProductName };
            await _context.Products.AddAsync(product);

            OrderItem orderItem = new OrderItem { Product = product, Quantity = orderRequestDto.ProductQuantity };
            await _context.OrderItems.AddAsync(orderItem);

            OrderRequest orderRequest = new OrderRequest
            {
                OrderItems = orderItem,
                DeliveryTime = orderRequestDto.RequestedDeliveryDay
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

            var product = orderRequest.OrderItems.Product;

            if (product == null)
            {
                product = new Product { Name = orderRequestDto.ProductName };
                await _context.Products.AddAsync(product);
            }
            else
            {
                product.Name = orderRequestDto.ProductName;
                _context.Products.Update(product);
            }

            orderRequest.OrderItems.Quantity = orderRequestDto.ProductQuantity;
            orderRequest.OrderItems.Product = product;

            _context.OrderItems.Update(orderRequest.OrderItems);

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
    public async Task<OrderRequest> UpdateOrderStatus(int orderRequestId, string newStatus)
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
            return orderRequest;
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
