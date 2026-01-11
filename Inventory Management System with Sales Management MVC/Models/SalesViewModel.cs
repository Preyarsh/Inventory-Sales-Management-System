namespace Inventory_Management_System_with_Sales_Management_MVC.Models
{
    public class SalesViewModel
    {
        public int SaleId { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime InvoiceDate { get; set; }

        public string CustomerName { get; set; }

        public decimal? GrandTotal { get; set; }

        public string PaymentStatus { get; set; }

        // Display name (Admin/User name)
        public string CreatedBy { get; set; }
    }
    public class SaleItemModel
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Rate { get; set; }
    }
    public class SaleCreateModel
    {
        public DateTime InvoiceDate { get; set; }

        public string CustomerName { get; set; }
        public string? CustomerMobile { get; set; }

        public decimal SubTotal { get; set; }
        public decimal Discount { get; set; }
        public decimal Tax { get; set; }
        public decimal GrandTotal { get; set; }

        public string? PaymentMode { get; set; }

        public int CreatedBy { get; set; }

        public List<SaleItemModel> Items { get; set; } = new();
    }
}
