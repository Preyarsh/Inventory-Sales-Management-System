namespace Inventory_Management_System_with_Sales_Management_API.Models
{
    public class SalesOrderModel
    {
        public int SalesOrderId { get; set; }
        public string? OrderNo { get; set; }
        public DateTime OrderDate { get; set; }
        public int CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerMobile { get; set; }

        public decimal SubTotal { get; set; }
        public decimal Discount { get; set; }
        public decimal Tax { get; set; }
        public decimal GrandTotal { get; set; }
        public string? BillingAddress { get; set; }
        public string? Status { get; set; } // Pending, Cancelled, Converted
        public int CreatedBy { get; set; }

        public List<SalesOrderItemModel> Items { get; set; } = new();
    }
    public class ConvertInvoiceResult
    {
        public int SaleId { get; set; }
        public string InvoiceNo { get; set; }
    }
}
