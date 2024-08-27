using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderProcess.Business.Services;
using OrderProcess.Entities.Dtos;
using OrderProcess.Entities.Entities;
using OrderProcess.Entities.Enums;

namespace OrderProcessWebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderRequestController : ControllerBase
{
    private readonly OrderRequestService _orderRequestService;

    public OrderRequestController(OrderRequestService orderRequestService)
    {
        _orderRequestService = orderRequestService;
    }

    [HttpPost]
    public IActionResult CreateOrderRequest(OrderRequestDTO orderRequestDTO)
    {
        if (orderRequestDTO == null) return BadRequest("Invalid request");

        var orderRequest = _orderRequestService.CreateOrderRequest(orderRequestDTO);

        return Ok(orderRequest.Result);

    }

    [HttpGet]
    public async Task<IActionResult> GetAllOrderRequests()
    {
        var orderRequests = await _orderRequestService.GetAllOrderRequests();
        if (orderRequests == null || !orderRequests.Any())
        {
            return NotFound("No order requests found.");
        }

        return Ok(orderRequests);
    }

    [HttpGet("status/{status}")]
    public async Task<IActionResult> GetOrderRequestsByStatus(string status)
    {
        if (string.IsNullOrEmpty(status))
        {
            return BadRequest("Status cannot be null or empty.");
        }

        try
        {
            var orderRequests = await _orderRequestService.GetOrderRequestsByStatus(status);
            return Ok(orderRequests);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateOrderRequest(int id, [FromBody] OrderRequestDTO orderRequestDTO)
    {
        if (orderRequestDTO == null) return BadRequest("Invalid request");

        try
        {
            var updatedOrderRequest = await _orderRequestService.UpdateOrderRequest(id, orderRequestDTO);

            if (updatedOrderRequest == null) return NotFound("Order request not found");

            return Ok(updatedOrderRequest);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] string newStatus)
    {
        if (string.IsNullOrEmpty(newStatus))
        {
            return BadRequest("Status cannot be null or empty.");
        }
        if (newStatus != OrderStatusEnum.offerExpected && newStatus != OrderStatusEnum.offerSubmitted)
        {
            return BadRequest("Wrong Status");
        }
        try
        {
            bool updatedOrderRequest = await _orderRequestService.UpdateOrderStatus(id, newStatus);

            if (!updatedOrderRequest) return NotFound("Order Not Found");

            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}