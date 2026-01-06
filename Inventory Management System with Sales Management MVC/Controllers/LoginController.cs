using Inventory_Management_System_with_Sales_Management_MVC.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using Microsoft.AspNetCore.Identity;
using System.Diagnostics;

namespace Inventory_Management_System_with_Sales_Management_MVC.Controllers
{
    public class LoginController : Controller
    {
        private readonly HttpClient _httpClient;

        public LoginController(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://localhost:44330/api/");
        }
        //public string HashPassword(string password)
        //{
        //    var hasher = new PasswordHasher<object>();
        //    return hasher.HashPassword(null, password);
        //}

        // Better token handling method
        private bool AddTokenToHeader()
        {
            var token = HttpContext.Session.GetString("Token");

            if (string.IsNullOrEmpty(token))
            {
                return false; // No token available
            }

            // Clear existing headers
            _httpClient.DefaultRequestHeaders.Authorization = null;
            _httpClient.DefaultRequestHeaders.Remove("Authorization");

            // Add new Authorization header
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            Console.WriteLine($"Token added to header: {token.Substring(0, Math.Min(20, token.Length))}...");
            return true;
        }

        [HttpGet]
        public async Task<IActionResult> GetUserProfile()
        {
            // Check if user is logged in
            var token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(token))
            {
                TempData["Error"] = "Please login first.";
                return RedirectToAction("Login");
            }

            // Add token to header
            if (!AddTokenToHeader())
            {
                TempData["Error"] = "Session expired. Please login again.";
                return RedirectToAction("Login");
            }

            try
            {
                var response = await _httpClient.GetAsync("User/Profile");
                var body = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"Profile Response Status: {response.StatusCode}");
                Console.WriteLine($"Profile Response Body: {body}");

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    HttpContext.Session.Remove("Token");
                    HttpContext.Session.Remove("UserID");
                    HttpContext.Session.Remove("Email");
                    TempData["Error"] = "Session expired. Please login again.";
                    return RedirectToAction("Login");
                }

                if (!response.IsSuccessStatusCode)
                {
                    TempData["Error"] = "Failed to load profile.";
                    return RedirectToAction("Login");
                }

                var result = JsonConvert.DeserializeObject<UserModel>(body);
                return View(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                TempData["Error"] = "An error occurred while loading profile.";
                return RedirectToAction("Login");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(UserModel model)
        {
            var token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(token))
            {
                TempData["Error"] = "Please login first.";
                return RedirectToAction("Login");
            }

            if (!AddTokenToHeader())
            {
                TempData["Error"] = "Session expired. Please login again.";
                return RedirectToAction("Login");
            }

            var json = JsonConvert.SerializeObject(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PutAsync("User/UpdateProfile", content);
                var body = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    HttpContext.Session.Remove("Token");
                    TempData["Error"] = "Session expired. Please login again.";
                    return RedirectToAction("Login");
                }

                if (!response.IsSuccessStatusCode)
                {
                    TempData["Error"] = "Failed to update profile.";
                    return RedirectToAction("GetUserProfile");
                }

                TempData["Success"] = "Profile updated successfully!";
                return RedirectToAction("GetUserProfile");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                TempData["Error"] = "An error occurred while updating profile.";
                return RedirectToAction("GetUserProfile");
            }
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                TempData["Error"] = "Email and password are required.";
                return RedirectToAction("Login");
            }

            var payload = new
            {
                Email = email,
                Password = password
            };
            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync("User/Login", content);
                var body = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"Login Status Code: {response.StatusCode}");
                Console.WriteLine($"Login Response Body: {body}");

                var loginResult = JsonConvert.DeserializeObject<LoginResponse>(body);

                if (!response.IsSuccessStatusCode)
                {
                    TempData["Error"] = "Invalid email or password.";
                    return RedirectToAction("Login");
                }

                if (loginResult == null || loginResult.User == null)
                {
                    ModelState.AddModelError("", "Invalid response from server.");
                    return View();
                }

                int userId = loginResult.User.UserID ?? 0;
                string token = loginResult.Token;
                bool isAdmin = loginResult.User.IsAdmin;

                // Store in session
                HttpContext.Session.SetString("UserID", userId.ToString());
                HttpContext.Session.SetString("JWTToken", token);
                HttpContext.Session.SetString("FullName", loginResult.User.FullName);
                HttpContext.Session.SetString("Email", loginResult.User.Email);
                HttpContext.Session.SetString("IsAdmin", isAdmin.ToString());
                HttpContext.Session.SetString("Token", token);

                Console.WriteLine($"Token stored in session: {token.Substring(0, Math.Min(20, token.Length))}...");

                if (isAdmin)
                    return RedirectToAction("Index", "Admin");
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login Error: {ex.Message}");
                TempData["Error"] = "An error occurred during login.";
                return RedirectToAction("Login");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Register(string fullName, string email, string password, string confirmPassword, bool fromAdmin = false)
        {
            
            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "Full Name and Email are required.";
                return fromAdmin
                ? RedirectToAction("UserList", "User")
                : RedirectToAction("Login");
            }

            if (password != confirmPassword)
            {
                TempData["Error"] = "Passwords do not match!";
                return fromAdmin
                ? RedirectToAction("UserList", "User")
                : RedirectToAction("Login");
            }
            //string passwordHash = HashPassword(password);
            var payload = new
            {
                FullName = fullName,
                Email = email,
                PasswordHash = password,
                IsActive = true,  // Set to 1 (true) so user is active after registration
                IsAdmin = false,
                
            };

            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var stopwatch = Stopwatch.StartNew();
                var response = await _httpClient.PostAsync("User/Register", content);
                var responseBody = await response.Content.ReadAsStringAsync();
                stopwatch.Stop();
                Console.WriteLine($"API call took: {stopwatch.ElapsedMilliseconds} ms");
         
                // Check for 400 error
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    TempData["Error"] = responseBody;
                    return fromAdmin
                        ? RedirectToAction("UserList", "User")
                        : RedirectToAction("Login");
                }

                if (!response.IsSuccessStatusCode)
                {
                    TempData["Error"] = $"Error {response.StatusCode}: {responseBody}";
                    return fromAdmin
                        ? RedirectToAction("UserList", "User")
                        : RedirectToAction("Login");
                }

                var result = JsonConvert.DeserializeObject<RegisterResponse>(responseBody);

                if (result == null || !result.Success)
                {
                    TempData["Error"] = result?.Message ?? "Registration failed.";
                    return fromAdmin
                        ? RedirectToAction("UserList", "User")
                        : RedirectToAction("Login");
                }

                TempData["Success"] = fromAdmin
                    ? "User added successfully."
                    : "Registration successful! Please login.";
                return fromAdmin
                        ? RedirectToAction("UserList", "User")
                        : RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Register Exception: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                TempData["Error"] = $"Error: {ex.Message}";
                return fromAdmin
                        ? RedirectToAction("UserList", "User")
                        : RedirectToAction("Login");
            }
        }
    }
}