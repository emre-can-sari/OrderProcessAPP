using OrderProcess.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderProcess.Entities.Entities;

public class OrderStatus
{
    public int Id { get; set; }
    public string Status { get; set; } = OrderStatusEnum.offerExpected;

}
