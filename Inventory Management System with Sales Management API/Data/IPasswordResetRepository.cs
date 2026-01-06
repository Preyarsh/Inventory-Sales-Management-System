using System;

namespace Inventory_Management_System_with_Sales_Management_API.Data
{
    public interface IPasswordResetRepository
    {
        bool InsertPasswordResetOTP(string email, string otp, DateTime expiryTime);
        bool UpdatePassword(string email, string newPassword);
        bool VerifyOTP(string email, string otp);
        bool ResetPasswordByUserId(int userId, string newPassword);
    }
}
