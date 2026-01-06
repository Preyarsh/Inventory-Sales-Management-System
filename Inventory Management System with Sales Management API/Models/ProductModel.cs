namespace Inventory_Management_System_with_Sales_Management_API.Models
{
    public class ProductModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }

        public int CategoryId { get; set; }
        public int UnitId { get; set; }

        public decimal PurchasePrice { get; set; }
        public decimal SalePrice { get; set; }
        public decimal OpeningStock { get; set; }
        public decimal CurrentStock { get; set; }

        public bool IsActive { get; set; }
        public string? CategoryName { get; set; }
        public string? UnitName { get; set; }
    }
    public class ProductListModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }

        public string CategoryName { get; set; }
        public string UnitName { get; set; }

        public decimal SalePrice { get; set; }
        public decimal CurrentStock { get; set; }
        public bool IsActive { get; set; }
    }

}
