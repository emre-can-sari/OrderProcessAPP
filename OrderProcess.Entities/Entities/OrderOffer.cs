using OrderProcess.Entities.Dtos;
using OrderProcess.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderProcess.Entities.Entities;

public class OrderOffer
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int OrderRequestId { get; set; }
    public List<OrderOfferItem> OrderOfferItems { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime DeliveryTime { get; set; }
    public string Status { get; set; } = OrderStatusEnum.offerSubmitted;

}
