using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace Inventory_Management_System_with_Sales_Management_API.Services
{
    public class EmailService : IEmailService // ✅ Implements Interface
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<bool> SendOTPEmailAsync(string toEmail, string otp)
        {
            try
            {
                var emailSettings = _configuration.GetSection("SmtpSettings");
                string smtpServer = emailSettings["Server"];
                int smtpPort = int.Parse(emailSettings["Port"]);
                string senderEmail = emailSettings["SenderEmail"];
                string senderName = emailSettings["SenderName"];
                string password = emailSettings["Password"];

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(senderName, senderEmail));
                message.To.Add(new MailboxAddress(toEmail, toEmail));
                message.Subject = "Verify Your Forgot Password with OTP";
                string emailBody = $@"
                    <div style='max-width: 600px; margin: auto; padding: 20px; font-family: Arial, sans-serif; text-align: center; background-color: #f9f9f9; border-radius: 10px;'>
                        <img src='https://res.cloudinary.com/dsagjcm6r/image/upload/v1737115937/Image/vzwpkplmljixvbp6iv7v.png' alt='Logo' style='width: 150px; margin-bottom: 20px;'>
                        <h2 style='color: #333;'>Verify Your Forgot Password</h2>
                        <p style='color: #555;'>We have received a Forgot Password attempt with the following code. Please enter it in the browser window where you started forgot password.</p>
                        <div style='background-color: #eee; padding: 15px; font-size: 24px; font-weight: bold; border-radius: 5px; display: inline-block;'>{otp}</div>
                        <p style='color: #777; margin-top: 20px;'>If you did not attempt to forgot password but received this email, please disregard it. The code will remain active for 5 minutes.</p>
                        <hr style='margin: 20px 0; border: none; border-top: 1px solid #ddd;'>
                        <p style='font-size: 14px; color: #888;'>© 2025 BOOKSAW. All rights reserved.</p>
                    </div>";
                message.Body = new TextPart("html")
                {
                    Text = emailBody
                };

                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync(smtpServer, smtpPort, false);
                    await client.AuthenticateAsync(senderEmail, password);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Email sending failed: {ex.Message}");
                return false;
            }
        }
    }
}
