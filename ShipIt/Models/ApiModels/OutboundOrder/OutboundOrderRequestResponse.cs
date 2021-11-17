using System.Collections.Generic;

namespace ShipIt.Models.ApiModels
{
    public class OrdersByTruck
    {
        public int TruckNumber { get; set; }

        public List<OrderLine> Orders { get; set; }
        public decimal TruckLoadInKg { get; set; }
    }
    public class OutboundOrderRequestResponse: Response
    {
        public int TrucksNeeded { get; set; }
        public List<OrdersByTruck> OrdersByTruck { get; set; }

        //Empty constructor required for xml serialization.
        public OutboundOrderRequestResponse() {}
    }
}