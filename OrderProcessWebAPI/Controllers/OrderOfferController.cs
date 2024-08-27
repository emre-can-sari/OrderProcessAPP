using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderProcess.Business.Services;
using OrderProcess.Entities.Dtos;

namespace OrderProcessWebAPI.Controllers;



[ApiController]
[Route("api/[controller]")]
public class OrderOfferController : ControllerBase
{
    private readonly OrderOfferService _orderOfferService;

    public OrderOfferController(OrderOfferService orderOfferService)
    {
        _orderOfferService = orderOfferService;
    }

    [HttpPost]
    [Authorize(Roles = "user")]
    public async Task<IActionResult> CreateOrderOffer(OrderOfferDTO orderOfferDto)
    {
        if (orderOfferDto == null)
            return BadRequest("Invalid request");

        try
        {
            var orderOffer = await _orderOfferService.CreateOrderOffer(orderOfferDto);

            return Ok(orderOffer);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "An error occurred while creating the order offer.", Error = ex.Message });
        }
    }


    [HttpGet("myOffers")]
    [Authorize(Roles = "user")]
    public async Task<IActionResult> GetOrderOffersByUserId()
    {
        try
        {
            var orderOffers = await _orderOfferService.GetOrderOffersByUserId();
            return Ok(orderOffers);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "An error occurred while retrieving order offers.", Error = ex.Message });
        }
    }

    [HttpGet("GetOffersByRequest/{orderRequestId}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> GetOrderOffersByOrderRequestId(int orderRequestId)
    {
        try
        {
            var orderOffers = await _orderOfferService.GetOrderOffersByOrderRequestId(orderRequestId);
            return Ok(orderOffers);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "An error occurred while retrieving order offers.", Error = ex.Message });
        }
    }
    [HttpPost("{id}/accept")]
    [Authorize(Roles = "admin")] 
    public async Task<IActionResult> AcceptOffer(int id)
    {
        try
        {
            var result = await _orderOfferService.AcceptOffer(id);

            if (result)
            {
                return Ok(new { Message = "Offer accepted successfully." });
            }
            else
            {
                return NotFound(new { Message = "Offer or associated order request not found." });
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "An error occurred while processing your request.", Error = ex.Message });
        }
    }
}
