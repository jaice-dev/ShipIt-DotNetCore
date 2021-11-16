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

        public static List<OrdersByTruck> CalculateOrdersByTruck(int trucksNeeded, List<OrderLine> orderLines,
            Dictionary<string, Product> products)
        {
            //Allocate orders to each truck
            //Maximum weight per truck is 2000kg
            //Orders of a single product are loaded onto the same truck
            //(or as few trucks as possible)
            //try to keep the number of trucks we need as small as possible
            
            //Sort all orders by (weight of product * number ordered)
            //For each order:
            //  If Over 2000kg:
            //      fill one empty truck and add excess to first truck with enough available space
            //      (what if over 4000kg or higher?)
            //  If Under 2000kg:
            //      Assign to first truck with available space
            
            // This never splits up orders - what if splitting an order up would save an extra truck...
            // However, always ensures single products are loaded onto same truck (if under 2000kg)
            
            
            
            var ordersByTruck = new List<OrdersByTruck>();
            for (var i = 1; i == trucksNeeded; i++)
            {
                var orderWeight = orderLines.Select(line => line.quantity * products[line.gtin].Weight).Sum() / 1000;
                ordersByTruck.Add(new OrdersByTruck {TruckNumber = i, Orders = orderLines, TruckLoadInKg = orderWeight});
            }

            return ordersByTruck;
        }
    }
}