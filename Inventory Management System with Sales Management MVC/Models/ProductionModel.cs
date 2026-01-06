using Microsoft.AspNetCore.Mvc.Rendering;

namespace Inventory_Management_System_with_Sales_Management_MVC.Models
{
    public class ProductionModel
    {
        public int ProductionId { get; set; }

        public int ProductId { get; set; }
        public int QuantityProduced { get; set; }
        public DateTime? ProductionDate { get; set; }

        public string? BatchNo { get; set; }
        public string? Notes { get; set; }

        // Dropdown
        public List<SelectListItem>? Products { get; set; }
    }
    public class ProductionListViewModel
    {
        public int ProductionId { get; set; }
        public string ProductName { get; set; }
        public int QuantityProduced { get; set; }
        public string Notes { get; set; }

        public DateTime ProductionDate { get; set; }
        public string? BatchNo { get; set; }
        public string CreatedByName { get; set; }
    }

}
