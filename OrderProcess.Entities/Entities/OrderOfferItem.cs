using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderProcess.Entities.Entities;

public class OrderOfferItem
{
    public int Id { get; set; }
    public OrderItem OrderItem { get; set; }
    public decimal Price { get; set; }
}
