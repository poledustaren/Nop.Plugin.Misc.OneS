using Nop.Core.Domain.Orders;
using Nop.Plugin.Misc.OneS.Core.ExportOrders;
using Nop.Services.Events;

namespace Nop.Plugin.Misc.OneS.Consumers
{
    public class OrderEditEventConsumer : IConsumer<OrderEditEvent>
    {
        private readonly IExportOrder _exportOrder;

        public OrderEditEventConsumer(IExportOrder exportOrder)
        {
            _exportOrder = exportOrder;
        }
        public void HandleEvent(OrderEditEvent eventMessage)
        {
            //_exportOrder.ExportOrderToOneS(eventMessage.Order);
        }
    }
}