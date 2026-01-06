using System;
using System.ComponentModel.DataAnnotations;

namespace Inventory_Management_System_with_Sales_Management_MVC.Models
{
    public class RegisterResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    public class LoginResponse
    {
        public bool Success { get; set; }
        public string Token { get; set; }
        public UserModel User { get; set; }
    }
    public class UserModel
    {
        public int? UserID { get; set; } // Primary Key

        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string PasswordHash { get; set; }

        [Required(ErrorMessage = "Full Name is required")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "IsAdmin value is required")]
        public bool IsAdmin { get; set; }

        [Required(ErrorMessage = "IsActive value is required")]
        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }
        public string? Phone { get; set; }
        public DateTime? DOB { get; set; }
        public string? Address { get; set; }
    }
    public class UserEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        public string Email { get; set; }

        [Phone]
        public string? Phone { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public string? Address { get; set; }

        [Required(ErrorMessage = "Role is required")]
        public string Role { get; set; } // "Admin", "Manager", "Staff", "Customer"

        public bool IsActive { get; set; } = true;

        // Password fields - optional
        
        public string? NewPassword { get; set; }

    
        public string? ConfirmPassword { get; set; }
    }
    public class UserCreateViewModel
    {
        [Required]
        public string FullName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, MinLength(8)]
        public string Password { get; set; }

        [Required, Compare("Password")]
        public string ConfirmPassword { get; set; }
    }
}
