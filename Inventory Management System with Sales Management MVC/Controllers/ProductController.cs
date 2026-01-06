using ClosedXML.Excel;
using Inventory_Management_System_with_Sales_Management_MVC.Models;
using Inventory_Management_System_with_Sales_Management_MVC.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Data;
using System.Text;
using Microsoft.Data.SqlClient;
namespace Inventory_Management_System_with_Sales_Management_MVC.Controllers
{
    public class ProductController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ProductService _productService;
        private readonly string _connectionString;
        private readonly IConfiguration _configuration;
        public ProductController(IHttpClientFactory factory, ProductService productService, IConfiguration configuration)
        {
            _httpClient = factory.CreateClient();
            _httpClient.BaseAddress = new Uri("https://localhost:44330/api/");
            _productService = productService;
            _configuration = configuration;
        }

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

        // ✅ PRODUCT LIST
        public async Task<IActionResult> ProductList()
        {
            try
            {
                SetAuthHeader();

                var response = await _httpClient.GetAsync("Product");

                if (!response.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = "Failed to load products";
                    return View(new List<ProductListViewModel>());
                }

                var json = await response.Content.ReadAsStringAsync();
                var products = JsonConvert.DeserializeObject<List<ProductListViewModel>>(json)
                               ?? new List<ProductListViewModel>();

                // ✅ ADD CATEGORY FILTER DATA HERE
                var categories = products
                    .Select(p => p.CategoryName)
                    .Where(c => !string.IsNullOrEmpty(c))
                    .Distinct()
                    .OrderBy(c => c)
                    .ToList();

                ViewBag.Categories = categories;

                // ✅ VERY IMPORTANT
                return View(products);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View(new List<ProductListViewModel>());
            }
        }
        public async Task<IActionResult> AddProduct()
        {
            var model = new ProductFormViewModel();
            await LoadDropdowns(model);
            return View(model);
        }

        // ✅ POST: Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProduct(ProductFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdowns(model);
                return View(model);
            }

            try
            {
                SetAuthHeader();

                var json = JsonConvert.SerializeObject(model);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("Product", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Product added successfully";
                    return RedirectToAction("ProductList");
                }

                TempData["ErrorMessage"] = "Failed to add product";
                await LoadDropdowns(model);
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                await LoadDropdowns(model);
                return View(model);
            }
        }
        private async Task LoadDropdowns(ProductFormViewModel model)
        {
            SetAuthHeader();

            // Categories
            var catResponse = await _httpClient.GetAsync("Category/Dropdown");
            var catJson = await catResponse.Content.ReadAsStringAsync();
            var categories = JsonConvert.DeserializeObject<List<CategoryModel>>(catJson);

            model.Categories = categories
                .Select(c => new SelectListItem
                {
                    Value = c.CategoryId.ToString(),
                    Text = c.CategoryName
                }).ToList();

            // Units
            var unitResponse = await _httpClient.GetAsync("Unit/Dropdown");
            var unitJson = await unitResponse.Content.ReadAsStringAsync();
            var units = JsonConvert.DeserializeObject<List<UnitModel>>(unitJson);

            model.Units = units
                .Select(u => new SelectListItem
                {
                    Value = u.UnitID.ToString(),
                    Text = u.UnitName
                }).ToList();
        }
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                SetAuthHeader();

                var response = await _httpClient.DeleteAsync($"Product/{id}");

                if (response.IsSuccessStatusCode)
                {
                    return Json(new { success = true });
                }

                var error = await response.Content.ReadAsStringAsync();
                return Json(new { success = false, message = error });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        public IActionResult ExportProductsToExcel()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            DataTable dt = new DataTable();

            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("PR_Products_Export", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            using (var workbook = new XLWorkbook())
            {
                var sheet = workbook.Worksheets.Add("Products");
                sheet.Cell(1, 1).InsertTable(dt, "ProductsTable");
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
                        $"Products_{DateTime.Now:yyyyMMdd}.xlsx"
                    );
                }
            }
        }

    }
}
