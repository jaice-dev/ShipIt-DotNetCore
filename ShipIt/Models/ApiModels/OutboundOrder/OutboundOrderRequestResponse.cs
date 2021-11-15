namespace ShipIt.Models.ApiModels
{
    public class OutboundOrderRequestResponse: Response
    {
        public int TrucksNeeded { get; set; }
        
        //Empty constructor required for xml serialization.
        public OutboundOrderRequestResponse() {}
    }
}