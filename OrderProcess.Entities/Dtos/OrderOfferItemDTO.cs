using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrderProcess.Entities.Entities;

namespace OrderProcess.Entities.Dtos;

public class OrderOfferItemDTO
{
    public OrderItemDTO OrderItems { get; set; }
    public decimal Price { get; set; }
}
