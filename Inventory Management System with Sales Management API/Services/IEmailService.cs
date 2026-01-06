using System.Threading.Tasks;

namespace Inventory_Management_System_with_Sales_Management_API.Services
{
    public interface IEmailService
    {
        Task<bool> SendOTPEmailAsync(string toEmail, string otp);
    }
}
