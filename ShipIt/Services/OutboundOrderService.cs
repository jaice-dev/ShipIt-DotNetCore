using System;
using System.Collections.Generic;
using System.Linq;
using ShipIt.Models.ApiModels;

namespace ShipIt_DotNetCore.Services
{
    public class OutboundOrderService
    {
        public static List<OrdersByTruck> CalculateOrdersByTruck(List<OrderLine> orderLines,
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

            // trucknumber against weight left
            var truckCapacityDict = new Dictionary<int, float>();
            //TODO make a list

            // Sort all orders by (weight of product * number ordered)
            var sortedOrderLines = orderLines.OrderBy(o => o.quantity * products[o.gtin].Weight).Reverse();

            foreach (var orderLine in sortedOrderLines)
            {
                while (orderLine.quantity != 0)
                {
                    //If any existing truck has space, allocate there
                    if (truckCapacityDict.Any(i => i.Value > CalculateOrderWeight(products, orderLine)))
                    {
                        // truckCapacityDict.Where().OrderBy.First()
                        foreach (var entry in truckCapacityDict)
                        {
                            if (entry.Value > CalculateOrderWeight(products, orderLine))
                            {
                                AllocateOrderToExistingTruck(products, ordersByTruck, entry, orderLine,
                                    truckCapacityDict);
                            }
                        }
                    }
                    else
                    {
                        AllocateOrderToNewTruck( products, orderLine, ordersByTruck, truckCapacityDict);
                    }
                }
            }

            //have to add TruckLoadInKg to every line ordersByTruck
            foreach (var order in ordersByTruck)
            {
                order.TruckLoadInKg = order.Orders.Select(line => line.quantity * products[line.gtin].Weight).Sum() / 1000;
            }

            return ordersByTruck;
        }

        private static void AllocateOrderToExistingTruck(Dictionary<string, Product> products, List<OrdersByTruck> ordersByTruck, KeyValuePair<int, float> entry,
            OrderLine orderLine, Dictionary<int, float> truckCapacityDict)
        {
            var truckIndex = entry.Key;
            ordersByTruck[truckIndex - 1].Orders.Add(new OrderLine
                {gtin = orderLine.gtin, quantity = orderLine.quantity});
            truckCapacityDict[truckIndex] -= CalculateOrderWeight(products, orderLine);
            orderLine.quantity = 0;
        }

        private static void AllocateOrderToNewTruck(Dictionary<string, Product> products,
            OrderLine orderLine, List<OrdersByTruck> ordersByTruck,
            Dictionary<int, float> truckCapacityDict)
        {
            var truckNumber = CreateTruck(truckCapacityDict);
            var numberOfItemsOnTruck = Math.Min(CalculateMaxNumberOfItems(products, orderLine), orderLine.quantity);

            var order = new OrderLine() {gtin = orderLine.gtin, quantity = numberOfItemsOnTruck};
            ordersByTruck.Add(new OrdersByTruck {TruckNumber = truckNumber, Orders = new List<OrderLine> {order}});
            
            //Update truck dict to reflect space left
            truckCapacityDict[truckNumber] -= numberOfItemsOnTruck * (products[orderLine.gtin].Weight / 1000);

            //Update Orderline
            orderLine.quantity -= numberOfItemsOnTruck;
        }

        private static int CalculateMaxNumberOfItems(Dictionary<string, Product> products, OrderLine orderLine)
        {
            var maxNumberOfItems = (int) Math.Floor(2000 / (products[orderLine.gtin].Weight / 1000));
            return maxNumberOfItems;
        }

        private static int CreateTruck(Dictionary<int, float> truckDict)
        {
            var numberOfTrucks = truckDict.Count;
            truckDict.Add(numberOfTrucks + 1, 2000);
            return truckDict.Count;
        }

        private static float CalculateOrderWeight(Dictionary<string, Product> products, OrderLine orderLine)
        {
            return orderLine.quantity * products[orderLine.gtin].Weight / 1000;
        }
    }
}