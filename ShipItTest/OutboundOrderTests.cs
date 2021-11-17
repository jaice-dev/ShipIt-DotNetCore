using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ShipIt.Controllers;
using ShipIt.Exceptions;
using ShipIt.Models.ApiModels;
using ShipIt.Models.DataModels;
using ShipIt.Repositories;
using ShipItTest.Builders;

namespace ShipItTest
{
    public class OutboundOrderControllerTests : AbstractBaseTest
    {
        OutboundOrderController outboundOrderController = new OutboundOrderController(
            new StockRepository(),
            new ProductRepository()
        );
        StockRepository stockRepository = new StockRepository();
        CompanyRepository companyRepository = new CompanyRepository();
        ProductRepository productRepository = new ProductRepository();
        EmployeeRepository employeeRepository = new EmployeeRepository();

        private static Employee EMPLOYEE = new EmployeeBuilder().CreateEmployee();
        private static Company COMPANY = new CompanyBuilder().CreateCompany();
        private static readonly int WAREHOUSE_ID = EMPLOYEE.WarehouseId;

        private Product product;
        private int productId;
        private const string GTIN = "0000";
        
        private Product product2;
        private int productId2;
        private const string GTIN2 = "0001";

        public new void onSetUp()
        {
            base.onSetUp();
            employeeRepository.AddEmployees(new List<Employee>() { EMPLOYEE });
            companyRepository.AddCompanies(new List<Company>() { COMPANY });
            var productDataModel = new ProductBuilder().setGtin(GTIN).CreateProductDatabaseModel();
            var productDataModel2 = new ProductBuilder().setGtin(GTIN2).CreateProductDatabaseModel();
            productRepository.AddProducts(new List<ProductDataModel>() { productDataModel, productDataModel2 });
            product = new Product(productRepository.GetProductByGtin(GTIN));
            product2 = new Product(productRepository.GetProductByGtin(GTIN2));
            productId = product.Id;
            productId2 = product2.Id;
        }

        [Test]
        public void TestOutboundOrder()
        {
            onSetUp();
            stockRepository.AddStock(WAREHOUSE_ID, new List<StockAlteration>() { new StockAlteration(productId, 10) });
            var outboundOrder = new OutboundOrderRequestModel()
            {
                WarehouseId = WAREHOUSE_ID,
                OrderLines = new List<OrderLine>()
                {
                    new OrderLine()
                    {
                        gtin = GTIN,
                        quantity = 3
                    }
                }
            };

            var res = outboundOrderController.Post(outboundOrder);
            var stock = stockRepository.GetStockByWarehouseAndProductIds(WAREHOUSE_ID, new List<int>() { productId })[productId];
            Assert.AreEqual(stock.held, 7);
        }

        [Test]
        public void TestOutboundOrderInsufficientStock()
        {
            onSetUp();
            stockRepository.AddStock(WAREHOUSE_ID, new List<StockAlteration>() { new StockAlteration(productId, 10) });
            var outboundOrder = new OutboundOrderRequestModel()
            {
                WarehouseId = WAREHOUSE_ID,
                OrderLines = new List<OrderLine>()
                {
                    new OrderLine()
                    {
                        gtin = GTIN,
                        quantity = 11
                    }
                }
            };

            try
            {
                outboundOrderController.Post(outboundOrder);
                Assert.Fail("Expected exception to be thrown.");
            }
            catch (InsufficientStockException e)
            {
                Assert.IsTrue(e.Message.Contains(GTIN));
            }
        }

        [Test]
        public void TestOutboundOrderStockNotHeld()
        {
            onSetUp();
            var noStockGtin = GTIN + "XYZ";
            productRepository.AddProducts(new List<ProductDataModel>() { new ProductBuilder().setGtin(noStockGtin).CreateProductDatabaseModel() });
            stockRepository.AddStock(WAREHOUSE_ID, new List<StockAlteration>() { new StockAlteration(productId, 10) });

            var outboundOrder = new OutboundOrderRequestModel()
            {
                WarehouseId = WAREHOUSE_ID,
                OrderLines = new List<OrderLine>()
                {
                    new OrderLine()
                    {
                        gtin = GTIN,
                        quantity = 8
                    },
                    new OrderLine()
                    {
                        gtin = noStockGtin,
                        quantity = 1000
                    }
                }
            };

            try
            {
                outboundOrderController.Post(outboundOrder);
                Assert.Fail("Expected exception to be thrown.");
            }
            catch (InsufficientStockException e)
            {
                Assert.IsTrue(e.Message.Contains(noStockGtin));
                Assert.IsTrue(e.Message.Contains("no stock held"));
            }
        }

        [Test]
        public void TestOutboundOrderBadGtin()
        {
            onSetUp();
            var badGtin = GTIN + "XYZ";

            var outboundOrder = new OutboundOrderRequestModel()
            {
                WarehouseId = WAREHOUSE_ID,
                OrderLines = new List<OrderLine>()
                {
                    new OrderLine()
                    {
                        gtin = GTIN,
                        quantity = 1
                    },
                    new OrderLine()
                    {
                        gtin = badGtin,
                        quantity = 1
                    }
                }
            };

            try
            {
                outboundOrderController.Post(outboundOrder);
                Assert.Fail("Expected exception to be thrown.");
            }
            catch (NoSuchEntityException e)
            {
                Assert.IsTrue(e.Message.Contains(badGtin));
            }
        }

        [Test]
        public void TestOutboundOrderDuplicateGtins()
        {
            onSetUp();
            stockRepository.AddStock(WAREHOUSE_ID, new List<StockAlteration>() { new StockAlteration(productId, 10) });
            var outboundOrder = new OutboundOrderRequestModel()
            {
                WarehouseId = WAREHOUSE_ID,
                OrderLines = new List<OrderLine>()
                {
                    new OrderLine()
                    {
                        gtin = GTIN,
                        quantity = 1
                    },
                    new OrderLine()
                    {
                        gtin = GTIN,
                        quantity = 1
                    }
                }
            };

            try
            {
                outboundOrderController.Post(outboundOrder);
                Assert.Fail("Expected exception to be thrown.");
            }
            catch (ValidationException e)
            {
                Assert.IsTrue(e.Message.Contains(GTIN));
            }
        }
        
        [Test]
        public void TestOutboundOrderTrucksNeededResponse()
        {
            onSetUp();
            stockRepository.AddStock(WAREHOUSE_ID, new List<StockAlteration>() { new StockAlteration(productId, 10000) });
            //Product weighs 300g - trucks hold 2,000,000g
            var outboundOrder = new OutboundOrderRequestModel()
            {
                WarehouseId = WAREHOUSE_ID,
                OrderLines = new List<OrderLine>()
                {
                    new OrderLine()
                    {
                        gtin = GTIN,
                        quantity = 10000
                        //weighs 3,000,000g
                    }
                }
            };

            var res = outboundOrderController.Post(outboundOrder);
            Assert.AreEqual(2, res.TrucksNeeded);
        }

        [Test]
        public void TestTruckAllocation()
        {
            onSetUp();
            stockRepository.AddStock(WAREHOUSE_ID, new List<StockAlteration>() { new StockAlteration(productId, 10000) });
            stockRepository.AddStock(WAREHOUSE_ID, new List<StockAlteration>() {new StockAlteration(productId2, 1000) });
            
            var outboundOrder = new OutboundOrderRequestModel()
            {
                WarehouseId = WAREHOUSE_ID,
                OrderLines = new List<OrderLine>()
                {
                    new OrderLine()
                    {
                        gtin = GTIN,
                        quantity = 10000
                        //weighs 3,000,000g
                    },
                    new OrderLine()
                    {
                        gtin = GTIN2,
                        quantity = 1000
                        //weighs 300,000g
                    }
                }
            };
            
            var res = outboundOrderController.Post(outboundOrder);
            Assert.AreEqual(2, res.TrucksNeeded);
            Assert.AreEqual(1, res.OrdersByTruck.First().TruckNumber);
            Assert.AreEqual(1999.8, res.OrdersByTruck.First().TruckLoadInKg);
            Assert.AreEqual(6666, res.OrdersByTruck.First().Orders.First().quantity);

            
        }
        
    }
}
