﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderProcess.Entities.Dtos;

public class OrderOfferDTO
{
    public List<OrderOfferItemDTO> orderItemDTO {  get; set; }
    public DateTime DeliveryTime { get; set; }
}
