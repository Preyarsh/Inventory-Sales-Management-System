using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Inventory_Management_System_with_Sales_Management_MVC.Models;

namespace Inventory_Management_System_with_Sales_Management_MVC.Controllers
{
    public class ForgetPasswordController : Controller
    {
        private readonly HttpClient _httpClient;

        public ForgetPasswordController(HttpClient httpClient)
        {
            // use injected HttpClient instance (do not recreate)
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _httpClient.BaseAddress = new Uri("https://localhost:44330/api/");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var payload = new
            {
                Email = model.Email
            };

            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync("PasswordReset/RequestPasswordReset", content);
                var responseBody = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"ForgotPassword Status: {response.StatusCode}");
                Console.WriteLine($"ForgotPassword Body: {responseBody}");

                if (!response.IsSuccessStatusCode)
                {
                    TempData["Error"] = "Email not found or failed to send OTP.";
                    return View(model);
                }

                // Ensure session is available in Program.cs: services.AddSession(); and app.UseSession();
                HttpContext.Session.SetString("ResetEmail", model.Email);

                TempData["Success"] = "OTP sent to your email.";
                return RedirectToAction("VerifyOtp");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ForgotPassword Error: {ex.Message}");
                TempData["Error"] = "Something went wrong. Please try again.";
                return View(model);
            }
        }
        [HttpGet]
        public IActionResult VerifyOtp()
        {
            var email = HttpContext.Session.GetString("ResetEmail");

            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("ForgotPassword");
            }

            var model = new VerifyOTPViewModel
            {
                Email = email
            };

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> VerifyOtp(VerifyOTPViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var payload = new
            {
                Email = model.Email,
                Otp = model.OTP
            };

            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("PasswordReset/VerifyOTP", content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("OTP", "Invalid or expired OTP");
                return View(model);
            }

            // OTP verified successfully
            HttpContext.Session.SetString("OtpVerified", "true");

            return RedirectToAction("ResetPassword");
        }
        [HttpPost]
        public async Task<IActionResult> ResendOTP([FromBody] VerifyOTPViewModel model)
        {
            var payload = new
            {
                Email = model.Email
            };

            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("PasswordReset/RequestPasswordReset", content);

            if (!response.IsSuccessStatusCode)
            {
                return Json(new { success = false });
            }

            return Json(new { success = true });
        }
        
        [HttpGet]
        public IActionResult ResetPassword()
        {
            var email = HttpContext.Session.GetString("ResetEmail");
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("ForgotPassword");

            return View(new ResetPasswordViewModel { Email = email });
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // 🔥 API EXPECTS password + confirmPassword
            var payload = new
            {
                email = model.Email,
                password = model.NewPassword,
                confirmPassword = model.ConfirmPassword
            };

            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("PasswordReset/UpdatePassword", content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = responseBody;
                return View(model);
            }

            TempData["Success"] = "Password reset successfully. Please login.";
            return RedirectToAction("Login", "Login");
        }
    }
}
