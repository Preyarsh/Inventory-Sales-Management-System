namespace Inventory_Management_System_with_Sales_Management_API.Models
{
    public class UserModel
    {
        public int UserID { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string FullName { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DOB { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
    }
    public class LoginModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
    public class UpdateUserModel
    {
        public int UserID { get; set; }

        public string FullName { get; set; }
        public string Email { get; set; }

        // OPTIONAL FIELDS
        public DateTime? DOB { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }

        public bool IsActive { get; set; }
        public bool IsAdmin { get; set; }
        public string? NewPassword { get; set; }
        public string? ConfirmPassword { get; set; }
    }
}
