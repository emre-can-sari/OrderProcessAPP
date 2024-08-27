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
    public OrderItem OrderItem { get; set; }
    public decimal Price { get; set; }
    public DateTime DeliveryTime { get; set; }
    public string Status { get; set; } = OrderStatusEnum.offerSubmitted;

}
