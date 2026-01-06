using System.ComponentModel.DataAnnotations;

namespace Inventory_Management_System_with_Sales_Management_MVC.Models
{
    public class UnitModel
    {
        public int UnitID { get; set; }

        [Required]
        public string UnitName { get; set; }

        [Required]
        public string UnitShortName { get; set; }

        public string Description { get; set; }

        public bool IsActive { get; set; }
    }
}
