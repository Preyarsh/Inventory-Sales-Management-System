using System;

namespace Inventory_Management_System_with_Sales_Management_API.Models
{
    public class EmailRequest
    {
        public string? Email { get; set; }
    }

    public class PasswordResetRequestModel
    {
        // Optional user id (kept for other flows)
        public string? UserId { get; set; }

        // Controller expects Email + OTP
        public string Email { get; set; }
        public string? OTP { get; set; }
        public DateTime ExpiryTime { get; set; }
    }

    // Matches controller UpdatePassword payload
    public class ResetPasswordRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
