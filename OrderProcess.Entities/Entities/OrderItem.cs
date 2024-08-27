using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderProcess.Entities.Entities;
public class OrderItem
{
    public int Id { get; set; }
    public decimal Quantity { get; set; }
    public Product Product { get; set; }
}