using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderProcess.Entities.Dtos;

public class OrderRequestDTO
{
    public List<OrderItemDTO> OrderItems { get; set; }
    public DateTime RequestedDeliveryDay { get; set; }
}
