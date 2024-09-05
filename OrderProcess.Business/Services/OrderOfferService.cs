using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
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
    private readonly HttpClient _httpClient;
    public OrderOfferService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor,HttpClient httpClient)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _httpClient = httpClient;
    }

    public async Task<OrderOffer> CreateOrderOffer(OrderOfferDTO orderOfferDto, int orderRequestId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var orderRequest = _context.OrderRequests.FirstOrDefault(x => x.Id == orderRequestId);
            if (orderRequest == null)
            {
                throw new Exception("Order Not Found");
            }
            if (orderRequest.Status == OrderStatusEnum.accepted)
            {
                throw new Exception("You cannot bid on completed orders");
            }

            List<OrderOfferItem> orderItems = new List<OrderOfferItem>();
            decimal totalPrice = 0;
            foreach (var productDto in orderOfferDto.orderItemDTO)
            {
                totalPrice += productDto.Price;
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.Name == productDto.Name);

                if (product == null)
                {
                    product = new Product { Name = productDto.Name };
                    _context.Products.Add(product);
                    await _context.SaveChangesAsync();
                }

                OrderItem orderItem = new OrderItem
                {
                    Product = product,
                    Quantity = productDto.Quantity
                };
                await _context.OrderItems.AddAsync(orderItem);
                await _context.SaveChangesAsync();

                OrderOfferItem orderOfferItem = new OrderOfferItem
                {
                    OrderItem = orderItem,
                    Price = productDto.Price
                };
                await _context.OrderOfferItems.AddAsync(orderOfferItem);

                orderItems.Add(orderOfferItem);
            }

            var claim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null && int.TryParse(claim.Value, out int userId))
            {
                OrderOffer orderOffer = new OrderOffer
                {
                    UserId = userId,
                    OrderRequestId = orderRequestId,
                    OrderOfferItems = orderItems,
                    TotalPrice = totalPrice,
                    DeliveryTime = orderOfferDto.DeliveryTime,
                };
                await _context.OrderOffers.AddAsync(orderOffer);
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
                .Include(o => o.OrderOfferItems)
               .ThenInclude(ooi => ooi.OrderItem)
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
               .Include(o => o.OrderOfferItems)
               .ThenInclude(ooi=> ooi.OrderItem)
               .ThenInclude(oi=> oi.Product)
               
                .Where(o => o.OrderRequestId == orderRequestId)
                .ToListAsync();
        }

    public async Task<bool> AcceptOffer(int id)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();


        try
        {
          

            var orderOffer = await _context.OrderOffers
                .Include (o => o.OrderOfferItems)
                .ThenInclude (ooi=> ooi.OrderItem)
                .ThenInclude(oi=> oi.Product)
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

            List<StockDTO> responseStockDTO = new List<StockDTO>();

            foreach (var offer in orderOffer.OrderOfferItems)
            {
                StockDTO stockDTO = new StockDTO
                {
                    Name = offer.OrderItem.Product.Name,
                    Quantity = offer.OrderItem.Quantity
                };
                responseStockDTO.Add(stockDTO);

            }


            var url = "https://localhost:7078/api/TempStock";

            var jsonContent = JsonConvert.SerializeObject(responseStockDTO);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);

            var responseContent = await response.Content.ReadAsStringAsync();

      



            if (response.IsSuccessStatusCode)
            {
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return true;

            }
            else
            {
                await transaction.RollbackAsync();
                return false;
            }
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw; 
        }
    }


}
