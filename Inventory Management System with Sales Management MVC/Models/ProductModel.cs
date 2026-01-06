using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Inventory_Management_System_with_Sales_Management_MVC.Models
{
    public class ProductModel
    {
    }
    public class ProductListViewModel
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
    public class ProductFormViewModel
    {
        public int ProductId { get; set; }

        [Required]
        public string ProductName { get; set; }

        public string ProductCode { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        public int UnitId { get; set; }

        [Required]
        public decimal PurchasePrice { get; set; }

        [Required]
        public decimal SalePrice { get; set; }

        public decimal OpeningStock { get; set; }
        public decimal CurrentStock { get; set; }

        public string UnitName { get; set; }
        public string CategoryName { get; set; }


        public bool IsActive { get; set; } = true;

        // 🔽 Dropdowns
        public List<SelectListItem> Categories { get; set; } = new();
        public List<SelectListItem> Units { get; set; } = new();
    }
}
