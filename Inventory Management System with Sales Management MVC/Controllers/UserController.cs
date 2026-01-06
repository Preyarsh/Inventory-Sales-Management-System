using ClosedXML.Excel;
using Inventory_Management_System_with_Sales_Management_MVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Data;
using System.Text;

namespace Inventory_Management_System_with_Sales_Management_MVC.Controllers
{
    public class UserController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public UserController(IHttpClientFactory factory, IConfiguration configuration)
        {
            _httpClient = factory.CreateClient();
            _httpClient.BaseAddress = new Uri("https://localhost:44330/api/User/");
            _configuration = configuration;
        }

        // ✅ Helper method to set JWT token
        private void SetAuthHeader()
        {
            var token = HttpContext.Session.GetString("JWTToken");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
        }

        // ✅ Index method (maps to /User/Index)
        public async Task<IActionResult> Index()
        {
            return await UserList();
        }

        // ✅ Get all users
        public async Task<IActionResult> UserList()
        {
            try
            {
                SetAuthHeader();
                var response = await _httpClient.GetAsync("Users");

                if (!response.IsSuccessStatusCode)
                {
                    return View("UserList", new List<UserModel>());
                }

                var json = await response.Content.ReadAsStringAsync();
                var users = JsonConvert.DeserializeObject<List<UserModel>>(json) ?? new List<UserModel>();

                return View("UserList", users);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Failed to load users: " + ex.Message;
                return View("UserList", new List<UserModel>());
            }
        }

        // ✅ GET Edit - Load user data
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            SetAuthHeader();

            var response = await _httpClient.GetAsync($"{id}");
            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Failed to load user";
                return RedirectToAction("Index");
            }

            var json = await response.Content.ReadAsStringAsync();
            var apiUser = JsonConvert.DeserializeObject<UserModel>(json);

            // ✅ MAP API → ViewModel
            var model = new UserEditViewModel
            {
                Id = apiUser.UserID ?? 0,
                Name = apiUser.FullName,
                Email = apiUser.Email,
                Phone = apiUser.Phone,
                DateOfBirth = apiUser.DOB,
                Address = apiUser.Address,
                IsActive = apiUser.IsActive,
                Role = apiUser.IsAdmin ? "Admin" : "User"
            };

            return View(model);
        }


        // ✅ POST Edit - Update user
        [HttpPost]
        public async Task<IActionResult> Edit(int id, UserEditViewModel model)
        {
            try
            {
                // 🔥 Fix automatic validation issue
                if (string.IsNullOrEmpty(model.NewPassword))
                {
                    ModelState.Remove("NewPassword");
                    ModelState.Remove("ConfirmPassword");
                }

                // ✅ Custom validation for optional password
                if (!string.IsNullOrEmpty(model.NewPassword) || !string.IsNullOrEmpty(model.ConfirmPassword))
                {
                    if (string.IsNullOrEmpty(model.NewPassword))
                        ModelState.AddModelError("NewPassword", "New password is required");

                    else if (string.IsNullOrEmpty(model.ConfirmPassword))
                        ModelState.AddModelError("ConfirmPassword", "Confirm password is required");

                    else if (model.NewPassword != model.ConfirmPassword)
                        ModelState.AddModelError("ConfirmPassword", "Passwords do not match");

                    else if (model.NewPassword.Length < 8)
                        ModelState.AddModelError("NewPassword", "Password must be at least 8 characters");
                }

                if (!ModelState.IsValid)
                    return View(model);

                SetAuthHeader();

                var payload = new
                {
                    UserID = model.Id,
                    FullName = model.Name,
                    Email = model.Email,
                    Phone = model.Phone ?? "",
                    DOB = model.DateOfBirth,
                    Address = model.Address ?? "",
                    IsActive = model.IsActive,
                    IsAdmin = model.Role == "Admin",
                    NewPassword = string.IsNullOrEmpty(model.NewPassword) ? null : model.NewPassword,
                    ConfirmPassword = string.IsNullOrEmpty(model.ConfirmPassword) ? null : model.ConfirmPassword
                };

                var content = new StringContent(
                    JsonConvert.SerializeObject(payload),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PutAsync($"{id}", content);

                if (!response.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = "Failed to update user";
                    return View(model);
                }

                TempData["SuccessMessage"] = "User updated successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View(model);
            }
        }


        // ✅ Delete user
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                SetAuthHeader();
                var response = await _httpClient.DeleteAsync($"{id}");

                if (!response.IsSuccessStatusCode)
                {
                    return Json(new { success = false, message = "Failed to delete user." });
                }

                return Json(new { success = true, message = "User deleted successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ✅ View user details
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                SetAuthHeader();
                var response = await _httpClient.GetAsync($"{id}");

                if (!response.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction("Index");
                }

                var json = await response.Content.ReadAsStringAsync();
                var user = JsonConvert.DeserializeObject<UserModel>(json);

                return View(user);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Failed to load user details.";
                return RedirectToAction("Index");
            }
        }
        public async Task<IActionResult> AddUser()
        {
            return View();
        }
        public IActionResult ExportUsersToExcel()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            DataTable dt = new DataTable();

            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("PR_Users_Export", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            using (var workbook = new XLWorkbook())
            {
                var sheet = workbook.Worksheets.Add("Users");
                sheet.Cell(1, 1).InsertTable(dt, "UsersTable");
                sheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return File(
                        stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"Users_{DateTime.Now:yyyyMMdd}.xlsx"
                    );
                }
            }
        }
    }
}