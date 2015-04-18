using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Misc.OneS.Models;
using Nop.Services.Catalog;
using Nop.Services.Localization;

namespace Nop.Plugin.Misc.OneS.Core.ExportOrders
{
    
    public class ExportOrder : IExportOrder
    {
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly IProductService _productService;
        private readonly string _pathToExport;

        public ExportOrder(ILocalizationService localizationService, IWorkContext workContext, IProductService productService,MiscOneSSettings miscOneSSettings)
        {
            _pathToExport = miscOneSSettings.PathToExchange + @"\Exchange\Export";
            _localizationService = localizationService;
            _workContext = workContext;
            _productService = productService;
        }

        public void ExportOrderToOneS(Order order)
        {
            var orderOneS = GetOrderInformation(order);
            orderOneS = GetOrderItems(order, orderOneS);
            var xDoc = GetOrderXml(orderOneS);
            SaveXml(order, xDoc);
        }

        private void SaveXml(Order order, string xDoc)
        {
            var destinationFilename = Path.Combine(_pathToExport,
                String.Format("order_{0}_{1}.xml", order.Id, DateTime.Now.ToString("yy.MM.dd_HH.mm.ss_ffff")));
            using (var outfile = new StreamWriter(destinationFilename))
            {
                outfile.Write(xDoc);
            }
        }

        private static string GetOrderXml(OrderOneS orderOneS)
        {
            var xOrder = GetXOrder(orderOneS);
            var xItems = GetXOrderItems(orderOneS);
            var xJobs = new XElement("Jobs");
            var xComments = new XElement("Comments");
            var xComment = new XElement("Comment");
            xComments.Add(xComment);
            xOrder.Add(xItems);
            xOrder.Add(xJobs);
            xOrder.Add(xComments);
            var xDoc = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>\r\n" + xOrder;
            return xDoc;
        }

        private static XElement GetXOrderItems(OrderOneS orderOneS)
        {
            var xItems = new XElement("Items");

            foreach (var orderItem in orderOneS.OrderItems)
            {
                var xItem = new XElement("Item");
                xItem.Add(new XAttribute("State.Id", orderItem.StateId??""));
                xItem.Add(new XAttribute("Id", orderItem.Id));
                xItem.Add(new XAttribute("Sum", orderItem.Sum));
                xItem.Add(new XAttribute("UnitTitle", orderItem.UnitTitle??""));
                xItem.Add(new XAttribute("Quantity", orderItem.Quantity));
                xItem.Add(new XAttribute("Price", orderItem.Price));
                xItem.Add(new XAttribute("Title", orderItem.Title??""));
                xItem.Add(new XAttribute("Model", orderItem.Model??""));
                xItem.Add(new XAttribute("Model.Id", orderItem.ModelId??""));
                xItem.Add(new XAttribute("Product", orderItem.Product??""));
                xItem.Add(new XAttribute("Product.Id", orderItem.ProductId??""));
                xItem.Add(new XAttribute("ProductMarka.Id", orderItem.ProductMarkaId??""));
                xItem.Add(new XAttribute("Storage.Id", orderItem.StorageId??""));
                xItems.Add(xItem);
            }
            return xItems;
        }

        private static XElement GetXOrder(OrderOneS orderOneS)
        {
            var xOrder = new XElement("Order");
            xOrder.Add(new XAttribute("CancelReason", orderOneS.CancelReason??""));
            xOrder.Add(new XAttribute("IsConditionalReserve", orderOneS.IsConditionalReserve));
            xOrder.Add(new XAttribute("StateDate", orderOneS.StateDate));
            xOrder.Add(new XAttribute("State", orderOneS.State??""));
            xOrder.Add(new XAttribute("State.Id", orderOneS.StateId??""));
            xOrder.Add(new XAttribute("ResponsibleUser", orderOneS.ResponsibleUser ?? ""));
            xOrder.Add(new XAttribute("ResponsibleUser.Id", orderOneS.ResponsibleUserId??""));
            xOrder.Add(new XAttribute("DeliveryDate", orderOneS.DeliveryDate??""));
            xOrder.Add(new XAttribute("DeliveryAdress", orderOneS.DeliveryAdress??""));
            xOrder.Add(new XAttribute("DeliveryCity", orderOneS.DeliveryCity??""));
            xOrder.Add(new XAttribute("DeliveryCity.Id", orderOneS.DeliveryCityId??""));
            xOrder.Add(new XAttribute("Delivery", orderOneS.Delivery??""));
            xOrder.Add(new XAttribute("Delivery.Id", orderOneS.DeliveryId??""));
            xOrder.Add(new XAttribute("IsPaid", orderOneS.IsPaid??""));
            xOrder.Add(new XAttribute("Advance", orderOneS.Advance??""));
            xOrder.Add(new XAttribute("Payment.Id", orderOneS.PaymentId??""));
            xOrder.Add(new XAttribute("CustomerNote", orderOneS.CostomerNote??""));
            xOrder.Add(new XAttribute("CustomerIsLegal", orderOneS.CostomerIsLegal));
            xOrder.Add(new XAttribute("CustomerEmail", orderOneS.CostomerEmail??""));
            xOrder.Add(new XAttribute("CustomerName", orderOneS.CostomerName??""));
            xOrder.Add(new XAttribute("CreationDate", orderOneS.CreationDate));
            xOrder.Add(new XAttribute("Id", orderOneS.Id));
            xOrder.Add(new XAttribute("Date", orderOneS.Date));
            return xOrder;
        }

        private OrderOneS GetOrderItems(Order order, OrderOneS orderOneS)
        {
            var orderItems = order.OrderItems;
            foreach (var orderItem in orderItems)
            {
                var parentProduct = _productService.GetProductById(orderItem.Product.ParentGroupedProductId);
                var manufacturer = _productService.GetProductById(parentProduct.ParentGroupedProductId);

                var orderItemOneS = new Models.OrderItem();
                orderItemOneS.Id = orderItem.OrderId;
                orderItemOneS.StateId = ""; //TODO:вопрос
                orderItemOneS.Sum = orderItem.PriceInclTax;
                orderItemOneS.UnitTitle = "шт"; //TODO: вопрос
                orderItemOneS.Quantity = orderItem.Quantity;
                orderItemOneS.Price = orderItem.UnitPriceExclTax;
                orderItemOneS.Title = orderItem.Product.Name;
                orderItemOneS.Model = orderItem.Product.Name;
                orderItemOneS.ModelId = orderItem.Product.Sku;
                orderItemOneS.Product = parentProduct.Name;
                orderItemOneS.ProductId = parentProduct.Sku;
                orderItemOneS.ProductMarkaId = manufacturer.Sku;

                var variantAttributeCombination = orderItem.Product.ProductVariantAttributeCombinations.FirstOrDefault();
                if (variantAttributeCombination != null)
                    orderItemOneS.StorageId = variantAttributeCombination.Sku;
                orderOneS.OrderItems.Add(orderItemOneS);
            }
            return orderOneS;
        }

        private OrderOneS GetOrderInformation(Order order)
        {
            var orderOneS = new OrderOneS();
            var shippingAdress = order.ShippingAddress;
            orderOneS.CancelReason = "";
            orderOneS.IsConditionalReserve = true; // TODO:Есть ли в резерве? Нужно ?
            orderOneS.State = order.OrderStatus.GetLocalizedEnum(_localizationService, _workContext);
                //TODO:проверить какие статусы в бериколесах
            orderOneS.StateId = order.OrderStatusId.ToString();
            orderOneS.StateDate = order.CreatedOnUtc;
            orderOneS.ResponsibleUser = "manager";
                //TODO:1)если заказ новый, то кто ответственный 2)Если заказ меняется пользователем в админке то нужно брать имя юзера. Важно ли это для 1С?
            orderOneS.ResponsibleUserId = "";
            orderOneS.DeliveryAdress = shippingAdress.City +" "+ shippingAdress.Address1;
            orderOneS.DeliveryDate = ""; //TODO: нужно ли?
            orderOneS.DeliveryCity = shippingAdress.City; //TODO: нужно?
            orderOneS.DeliveryCityId = ""; //TODO:нужно?
            orderOneS.Delivery = order.ShippingMethod;
            orderOneS.DeliveryId = ""; //TODO:вопрос
            orderOneS.IsPaid = order.PaymentStatus.GetLocalizedEnum(_localizationService, _workContext);
            orderOneS.Advance = ""; //TODO:вопрос
            orderOneS.Payment = order.PaymentMethodSystemName;
            orderOneS.PaymentId = ""; //TODO:вопрос
            orderOneS.CostomerIsLegal = false; //TODO:вопрос
            orderOneS.CostomerEmail = order.Customer.Email;
            orderOneS.CostomerPhone = order.Customer.ShippingAddress.PhoneNumber;
            orderOneS.CostomerName = order.Customer.ShippingAddress.FirstName +" "+ order.Customer.ShippingAddress.LastName;
            orderOneS.CreationDate = order.CreatedOnUtc; //TODO:вопрос
            orderOneS.Id = order.Id;
            orderOneS.Date = order.CreatedOnUtc; //TODO:вопрос
            return orderOneS;
        }
    }
}
