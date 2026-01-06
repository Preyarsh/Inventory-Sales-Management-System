using Inventory_Management_System_with_Sales_Management_API.Data;
using Inventory_Management_System_with_Sales_Management_API.Models;
using Inventory_Management_System_with_Sales_Management_API.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Inventory_Management_System_with_Sales_Management_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PasswordResetController : ControllerBase
    {
        private readonly IPasswordResetRepository _passwordResetRepository;
        private readonly IEmailService _emailService;

        public PasswordResetController(IPasswordResetRepository passwordResetRepository, IEmailService emailService)
        {
            _passwordResetRepository = passwordResetRepository;
            _emailService = emailService;
        }

        // ==========================================================
        // ⭐ STEP 1 — REQUEST OTP
        // ==========================================================
        [HttpPost("RequestPasswordReset")]
        public async Task<IActionResult> RequestPasswordReset([FromBody] PasswordResetRequestModel model)
        {
            if (string.IsNullOrEmpty(model.Email))
                return BadRequest("Email is required.");

            string otp = new Random().Next(100000, 999999).ToString();
            var expiryTime = DateTime.UtcNow.AddMinutes(5);
            bool isInserted = _passwordResetRepository.InsertPasswordResetOTP(model.Email, otp, expiryTime);

            if (!isInserted)
                return BadRequest("Failed to generate OTP. Email may not be registered.");

            bool emailSent = await _emailService.SendOTPEmailAsync(model.Email, otp);

            if (!emailSent)
                return StatusCode(500, "OTP generated but failed to send email.");

            return Ok(new
            {
                message = "OTP sent successfully.",
                expirationTime = expiryTime
            });
        }

        // ==========================================================
        // ⭐ STEP 2 — VERIFY OTP
        // ==========================================================
        [HttpPost("VerifyOTP")]
        public IActionResult VerifyOTP([FromBody] PasswordResetRequestModel model)
        {
            if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.OTP))
                return BadRequest(new { message = "Email and OTP are required." });

            bool isVerified = _passwordResetRepository.VerifyOTP(model.Email, model.OTP);

            if (!isVerified)
                return BadRequest(new { message = "Invalid or expired OTP." });

            return Ok(new { message = "OTP verified successfully." });
        }

        // ==========================================================
        // ⭐ STEP 3 — UPDATE PASSWORD
        // ==========================================================
        [HttpPost("UpdatePassword")]
        public IActionResult UpdatePassword([FromBody] ResetPasswordRequest model)
        {
            if (string.IsNullOrEmpty(model.Password) || string.IsNullOrEmpty(model.ConfirmPassword))
                return BadRequest(new { message = "Password and Confirm Password are required." });

            if (model.Password != model.ConfirmPassword)
                return BadRequest(new { message = "Password and Confirm Password do not match." });

            // Email must be included in request model
            if (string.IsNullOrEmpty(model.Email))
                return BadRequest(new { message = "Email is required." });
            var hasher = new PasswordHasher<UserModel>();
            string hashedPassword = hasher.HashPassword(null, model.Password);
            bool isUpdated = _passwordResetRepository.UpdatePassword(model.Email,hashedPassword);

            if (!isUpdated)
                return BadRequest(new { message = "Failed to update password. Email may not be registered." });

            return Ok(new { message = "Password updated successfully." });
        }
    }
}
