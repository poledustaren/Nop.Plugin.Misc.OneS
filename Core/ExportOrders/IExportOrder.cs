using Nop.Core.Domain.Orders;

namespace Nop.Plugin.Misc.OneS.Core.ExportOrders
{
    public interface IExportOrder
    {
        void ExportOrderToOneS(Order order);
    }
}
