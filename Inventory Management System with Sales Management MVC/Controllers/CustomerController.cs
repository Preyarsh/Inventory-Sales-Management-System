using ClosedXML.Excel;
using Inventory_Management_System_with_Sales_Management_MVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;
using System.Text;

namespace Inventory_Management_System_with_Sales_Management_MVC.Controllers
{
    public class CustomerController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public CustomerController(IHttpClientFactory factory, IConfiguration configuration)
        {
            _httpClient = factory.CreateClient();
            _httpClient.BaseAddress = new Uri("https://localhost:44330/api/customers/");
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

        // =============================
        // LIST
        // =============================
        //public async Task<IActionResult> CustomerList()
        //{
        //    return View();
        //}

        public async Task<IActionResult> CustomerList()
        {
            try
            {
                SetAuthHeader();
                var response = await _httpClient.GetAsync("");

                if (!response.IsSuccessStatusCode)
                {
                    return View("CustomerList", new List<CustomerModel>());
                }

                var json = await response.Content.ReadAsStringAsync();
                var customers = JsonConvert.DeserializeObject<List<CustomerModel>>(json) ?? new List<CustomerModel>();

                return View("CustomerList", customers);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Failed to load customers: " + ex.Message;
                return View("CustomerList", new List<CustomerModel>());
            }
        }


        // =============================
        // CREATE
        // =============================
        public IActionResult AddCustomer()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddCustomer(CustomerModel model)
        {
            try
            {
                SetAuthHeader();

                var json = JsonConvert.SerializeObject(model);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("", content);

                if (!response.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = "Failed to create customer.";
                    return View(model);
                }

                TempData["SuccessMessage"] = "Customer created successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View(model);
            }
        }

        // =============================
        // EDIT
        // =============================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            SetAuthHeader();

            var response = await _httpClient.GetAsync($"{id}");
            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Customer not found.";
                return RedirectToAction("Index");
            }

            var json = await response.Content.ReadAsStringAsync();
            var model = JsonConvert.DeserializeObject<CustomerModel>(json);

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(CustomerModel model)
        {
            try
            {
                SetAuthHeader();

                var json = JsonConvert.SerializeObject(model);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"{model.CustomerId}", content);

                if (!response.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = "Failed to update customer.";
                    return View(model);
                }

                TempData["SuccessMessage"] = "Customer updated successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View(model);
            }
        }

        // =============================
        // DETAILS
        // =============================
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                SetAuthHeader();

                var response = await _httpClient.GetAsync($"{id}");
                if (!response.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = "Customer not found.";
                    return RedirectToAction("Index");
                }

                var json = await response.Content.ReadAsStringAsync();
                var customer = JsonConvert.DeserializeObject<CustomerModel>(json);

                return View(customer);
            }
            catch
            {
                TempData["ErrorMessage"] = "Failed to load customer.";
                return RedirectToAction("Index");
            }
        }

        // =============================
        // DELETE (AJAX)
        // =============================
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                SetAuthHeader();
                var response = await _httpClient.DeleteAsync($"{id}");

                if (!response.IsSuccessStatusCode)
                {
                    return Json(new { success = false, message = "Delete failed" });
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // =============================
        // EXPORT TO EXCEL
        // =============================
        public IActionResult ExportCustomersToExcel()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            DataTable dt = new DataTable();

            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("PR_Customers_Export", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            using (var workbook = new XLWorkbook())
            {
                var sheet = workbook.Worksheets.Add("Customers");
                sheet.Cell(1, 1).InsertTable(dt, "CustomersTable");
                sheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return File(
                        stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"Customers_{DateTime.Now:yyyyMMdd}.xlsx"
                    );
                }
            }
        }
    }
}
