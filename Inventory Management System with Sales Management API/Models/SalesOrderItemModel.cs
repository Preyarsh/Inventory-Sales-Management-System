namespace Inventory_Management_System_with_Sales_Management_API.Models
{
    public class SalesOrderItemModel
    {
        public int SalesOrderItemId { get; set; }
        public int SalesOrderId { get; set; }

        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Rate { get; set; }
    }
}
