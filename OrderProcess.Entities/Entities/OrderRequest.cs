using OrderProcess.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderProcess.Entities.Entities;

public class OrderRequest
{
    public int Id { get; set; }
    public OrderItem OrderItems { get; set; }
    public string Status { get; set; } = OrderStatusEnum.offerExpected;
    public DateTime DeliveryTime { get; set; }
}
