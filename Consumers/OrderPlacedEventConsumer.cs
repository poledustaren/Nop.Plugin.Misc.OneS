using Nop.Core.Domain.Orders;
using Nop.Plugin.Misc.OneS.Core.ExportOrders;
using Nop.Services.Events;

namespace Nop.Plugin.Misc.OneS.Consumers
{
    public class OrderPlacedEventConsumer : IConsumer<OrderPlacedEvent>
    {
         private readonly IExportOrder _exportOrder;

         public OrderPlacedEventConsumer(IExportOrder exportOrder)
        {
            _exportOrder = exportOrder;
        }
        public void HandleEvent(OrderPlacedEvent eventMessage)
        {
            //_exportOrder.ExportOrderToOneS(eventMessage.Order);
        }
    }
}