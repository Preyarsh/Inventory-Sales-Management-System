using System.ComponentModel.DataAnnotations;

namespace Inventory_Management_System_with_Sales_Management_API.Models
{
    public class ProductionModel
    {
        public int ProductionId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public int QuantityProduced { get; set; }

        [Required]
        public DateTime ProductionDate { get; set; }

        public string? BatchNo { get; set; }
        public string? Notes { get; set; }

        public int CreatedBy { get; set; }
    }
    public class ProductionListDto
    {
        public int ProductionId { get; set; }
        public string ProductName { get; set; }
        public int QuantityProduced { get; set; }
        public DateTime ProductionDate { get; set; }
        public string? BatchNo { get; set; }
        public string? Notes { get; set; }
        public string CreatedByName { get; set; }
    }
}
