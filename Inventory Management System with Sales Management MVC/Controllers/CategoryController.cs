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
    public class CategoryController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public CategoryController(IHttpClientFactory factory, IConfiguration configuration)
        {
            _httpClient = factory.CreateClient();
            _httpClient.BaseAddress = new Uri("https://localhost:44330/api/Category/");
            _configuration = configuration;
        }
        private void SetAuthHeader()
        {
            var token = HttpContext.Session.GetString("JWTToken"); // ✅ FIXED

            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
        }
        // ✅ Category List
        public async Task<IActionResult> CategoryList()
        {
            try
            {
                SetAuthHeader();
                var response = await _httpClient.GetAsync("Category");

                if (!response.IsSuccessStatusCode)
                {
                    return View(new List<CategoryModel>());
                }

                var json = await response.Content.ReadAsStringAsync();
                var categories = JsonConvert.DeserializeObject<List<CategoryModel>>(json)
                                 ?? new List<CategoryModel>();

                return View(categories);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Failed to load categories: " + ex.Message;
                return View(new List<CategoryModel>());
            }
        }
        [HttpGet]
        public IActionResult AddCategory()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                SetAuthHeader();

                var json = JsonConvert.SerializeObject(model);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Category added successfully";
                    return RedirectToAction("CategoryList");
                }

                var error = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = "Failed to add category";

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View(model);
            }
        }
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                SetAuthHeader();

                // API endpoint: DELETE api/Category/{id}
                var response = await _httpClient.DeleteAsync($"{id}");

                if (response.IsSuccessStatusCode)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Category deleted successfully"
                    });
                }

                return Json(new
                {
                    success = false,
                    message = "Failed to delete category"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
        public IActionResult ExportProductsToExcel()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            DataTable dt = new DataTable();

            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("PR_Categories_Export", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            using (var workbook = new XLWorkbook())
            {
                var sheet = workbook.Worksheets.Add("Categorys");
                sheet.Cell(1, 1).InsertTable(dt, "CategorysTable");
                sheet.Columns().AdjustToContents();
                string password = $"INV-{DateTime.Now:yyyyMMdd}";
                sheet.Protect(password);
                workbook.Protect(password);
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return File(
                        stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"Categorys_{DateTime.Now:yyyyMMdd}.xlsx"
                    );
                }
            }
        }
        public IActionResult Edit()
        {
            return View();
        }
    }
}
