using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderProcess.Entities.Enums;

public static class OrderStatusEnum
{
    public const string offerExpected = "offerExpected";
    public const string offerSubmitted = "offerSubmitted";
    public const string accepted = "accepted";
    public const string unacceptable = "unacceptable";
}
