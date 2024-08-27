using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderProcess.Entities.Dtos;

public class OrderOfferDTO
{
    public int OrderRequestId { get; set; }
    public decimal Quantity { get; set; }
    public string ProductName { get; set; }
    public decimal Price { get; set; }
    public DateTime DeliveryTime { get; set; }
}
