using System;
using System.Collections.Generic;
using System.Linq;
using ShipIt.Models.ApiModels;
using ShipIt.Models.DataModels;

namespace ShipIt_DotNetCore.Services
{
    public class InboundOrdersServices
    {
        public static Dictionary<Company, List<InboundOrderLine>> GetOrderLinesByCompany(IEnumerable<InboundStockDataModel> allStock)
        {
            var orderlinesByCompany = new Dictionary<Company, List<InboundOrderLine>>();

            foreach (var product in allStock)
            {
                if (product.Held < product.LowerThreshold && product.Discontinued == 0)
                {
                    var orderQuantity = Math.Max(product.LowerThreshold * 3 - product.Held,
                        product.MinimumOrderQuantity);

                    Company company = new Company();
                    company.Gcp = product.Gcp;
                    company.Addr2 = product.Addr2;
                    company.Addr3 = product.Addr3;
                    company.Addr4 = product.Addr4;
                    company.PostalCode = product.PostalCode;
                    company.City = product.City;
                    company.Tel = product.Tel;
                    company.Mail = product.Mail;

                    if (!orderlinesByCompany.ContainsKey(company))
                    {
                        orderlinesByCompany.Add(company, new List<InboundOrderLine>());
                    }

                    orderlinesByCompany[company].Add(
                        new InboundOrderLine()
                        {
                            gtin = product.Gtin,
                            name = product.Name,
                            quantity = orderQuantity
                        });
                }
            }

            return orderlinesByCompany;
        }
        
        public static IEnumerable<OrderSegment> GetOrderSegments(Dictionary<Company, List<InboundOrderLine>> orderlinesByCompany)
        {
            var orderSegments = orderlinesByCompany.Select(ol => new OrderSegment()
            {
                OrderLines = ol.Value,
                Company = ol.Key
            });
            return orderSegments;
        }
    }
}