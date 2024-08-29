using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderProcess.Entities.Dtos
{
    public class OrderItemDTO
    {
        public string ProductName { get; set; }
        public decimal ProductQuantity { get; set; }
    }
}
