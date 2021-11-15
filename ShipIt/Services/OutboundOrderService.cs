using System;
using System.Collections.Generic;
using System.Linq;
using ShipIt.Models.ApiModels;

namespace ShipIt_DotNetCore.Services
{
    public class OutboundOrderService
    {
        public static int CalculateTrucksNeeded(List<OrderLine> orderLines, Dictionary<string, Product> products)
        {
            var orderWeight = orderLines.Select(line => line.quantity * products[line.gtin].Weight).Sum();
            var truckCapacity = 2000000;
            
            var trucksNeeded = (int) Math.Ceiling(orderWeight / truckCapacity);
            return trucksNeeded;
        }
    }
}