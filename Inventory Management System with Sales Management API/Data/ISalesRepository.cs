using Inventory_Management_System_with_Sales_Management_API.Models;
using System.Data;

namespace Inventory_Management_System_with_Sales_Management_API.Data
{
    public interface ISalesRepository
    {
        int CreateSale(SaleCreateModel model);
        List<SaleListModel> GetSales();
        InvoiceViewModel GetInvoice(int saleId);
    }
}
