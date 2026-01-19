namespace Inventory_Management_System_with_Sales_Management_MVC.Models
{
    public class CustomerModel
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerMobile { get; set; }
        public string Email { get; set; }
        public string BillingAddress { get; set; }
        public string ShippingAddress { get; set; }
        public string? GstNumber { get; set; }
        public bool IsActive { get; set; }
    }
}
