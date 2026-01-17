using Inventory_Management_System_with_Sales_Management_API.Models;

namespace Inventory_Management_System_with_Sales_Management_API.Data
{
    public interface ISalesOrderRepository
    {
        int CreateSalesOrder(SalesOrderModel model);
        List<SalesOrderModel> GetSalesOrders();
        SalesOrderModel GetSalesOrderById(int id);
        bool UpdateSalesOrder(SalesOrderModel model);
        bool CancelSalesOrder(int salesOrderId, int updatedBy);
        int ConvertToInvoice(int salesOrderId, int createdBy);
    }
}
