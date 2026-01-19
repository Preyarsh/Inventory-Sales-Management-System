namespace Inventory_Management_System_with_Sales_Management_API.Models
{
    public class CustomerModel
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerMobile { get; set; }
        public string Email { get; set; }
        public string BillingAddress { get; set; }
        public string ShippingAddress { get; set; }
        public string GSTNumber { get; set; }

        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
    public class CustomerLookupModel
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
    }
}
