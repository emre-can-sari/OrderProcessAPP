using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OrderProcess.DataAccess;
using OrderProcess.Entities.Dtos;
using OrderProcess.Entities.Entities;
using OrderProcess.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace OrderProcess.Business.Services;

public class OrderOfferService
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public OrderOfferService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<OrderOffer> CreateOrderOffer(OrderOfferDTO orderOfferDto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Name == orderOfferDto.ProductName);

            if (product == null)
            {

                product = new Product { Name = orderOfferDto.ProductName };
                _context.Products.Add(product);
            }

            var orderItem = new OrderItem { Product = product, Quantity = orderOfferDto.Quantity };
            _context.OrderItems.Add(orderItem);

            var claim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null && int.TryParse(claim.Value, out int userId))
            {
                var orderOffer = new OrderOffer
                {
                    UserId = userId,
                    OrderRequestId = orderOfferDto.OrderRequestId,
                    OrderItem = orderItem,
                    Price = orderOfferDto.Price,
                    DeliveryTime = orderOfferDto.DeliveryTime,
                };
                _context.OrderOffers.Add(orderOffer);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return orderOffer;
            }
            else
            {
                await transaction.RollbackAsync();
                throw new Exception("User ID is invalid or not found.");
            }
        }

        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new Exception("Order offer creation failed.", ex);
        }
    }
    public async Task<List<OrderOffer>> GetOrderOffersByUserId()
    {
        var claim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim != null && int.TryParse(claim.Value, out int userId))
        {
            var orderOffers = await _context.OrderOffers
                .Include(o => o.OrderItem) 
                .ThenInclude(oi => oi.Product)
                .Where(o => o.UserId == userId)
                .ToListAsync();

            return orderOffers;
        }
        else
        {
            throw new Exception("User ID is invalid or not found.");
        }
    }
     public async Task<List<OrderOffer>> GetOrderOffersByOrderRequestId(int orderRequestId)
        {
            return await _context.OrderOffers
                .Include(o => o.OrderItem)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.OrderRequestId == orderRequestId)
                .ToListAsync();
        }

    public async Task<bool> AcceptOffer(int id)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var orderOffer = await _context.OrderOffers
                .Include(o => o.OrderItem)
                .ThenInclude(oi => oi.Product)
                .SingleOrDefaultAsync(o => o.Id == id);

            if (orderOffer == null)
            {
                await transaction.RollbackAsync();
                return false;
            }

            orderOffer.Status = OrderStatusEnum.accepted;
            _context.OrderOffers.Update(orderOffer);

            var orderRequest = await _context.OrderRequests
                .SingleOrDefaultAsync(o => o.Id == orderOffer.OrderRequestId);

            if (orderRequest == null)
            {
                await transaction.RollbackAsync();
                return false;
            }

            orderRequest.Status = OrderStatusEnum.accepted;

            var otherOffers = await _context.OrderOffers
                .Where(o => o.OrderRequestId == orderOffer.OrderRequestId && o.Id != id)
                .ToListAsync();

            foreach (var offer in otherOffers)
            {
                offer.Status = OrderStatusEnum.unaccepted;
                _context.OrderOffers.Update(offer);
            }

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
            return true;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw; 
        }
    }


}
