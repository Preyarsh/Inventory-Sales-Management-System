using Inventory_Management_System_with_Sales_Management_MVC.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace Inventory_Management_System_with_Sales_Management_MVC.Controllers
{
    public class SalesController : Controller
    {
        private readonly HttpClient _httpClient;

        public SalesController(IHttpClientFactory factory)
        {
            _httpClient = factory.CreateClient();
            _httpClient.BaseAddress = new Uri("https://localhost:44330/api/");
        }

        // ================= AUTH HEADER =================
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

        // ================= SALES LIST =================
        public async Task<IActionResult> SalesList(
            string searchQuery,
            string paymentStatus,
            DateTime? fromDate,
            DateTime? toDate,
            int page = 1)
        {
            SetAuthHeader();

            var query = $"Sales?searchQuery={searchQuery}" +
                        $"&paymentStatus={paymentStatus}" +
                        $"&fromDate={fromDate}" +
                        $"&toDate={toDate}" +
                        $"&page={page}";

            var response = await _httpClient.GetAsync(query);
            var json = await response.Content.ReadAsStringAsync();

            var list = JsonConvert.DeserializeObject<List<SalesViewModel>>(json) ?? new();

            // ViewBag filters
            ViewBag.SearchQuery = searchQuery;
            ViewBag.PaymentStatus = paymentStatus;
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");

            // Pagination (basic – API paging later)
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = 1;
            ViewBag.TotalRecords = list.Count;
            ViewBag.StartRecord = list.Any() ? 1 : 0;
            ViewBag.EndRecord = list.Count;

            return View(list);
        }

        // ================= CREATE SALE (GET) =================
        [HttpGet]
        public IActionResult AddSales()
        {
            return View();
        }

        // ================= CREATE SALE (POST) =================
        [HttpPost]
        public async Task<IActionResult> AddSales(SaleCreateModel model)
        {
            SetAuthHeader();

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Invalid sale data";
                return View(model);
            }

            var json = JsonConvert.SerializeObject(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("Sales", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Sale created successfully";
                return RedirectToAction(nameof(SalesList));
            }

            TempData["ErrorMessage"] = await response.Content.ReadAsStringAsync();
            return View(model);
        }

        // ================= VIEW INVOICE =================
        //[HttpGet]
        //public async Task<IActionResult> Details(int id)
        //{
        //    SetAuthHeader();

        //    var response = await _httpClient.GetAsync($"Sales/{id}");
        //    var json = await response.Content.ReadAsStringAsync();

        //    var invoice = JsonConvert.DeserializeObject<SalesInvoiceViewModel>(json);

        //    return View(invoice);
        //}

        // ================= PRINT / DOWNLOAD INVOICE =================
        [HttpGet]
        public async Task<IActionResult> PrintInvoice(int id)
        {
            SetAuthHeader();

            var response = await _httpClient.GetAsync($"Sales/{id}/pdf");

            if (!response.IsSuccessStatusCode)
                return NotFound();

            var pdfBytes = await response.Content.ReadAsByteArrayAsync();
            return File(pdfBytes, "application/pdf", $"Invoice_{id}.pdf");
        }

        // ================= DELETE SALE (ADMIN) =================
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            SetAuthHeader();

            var response = await _httpClient.DeleteAsync($"Sales/{id}");

            return Json(new
            {
                success = response.IsSuccessStatusCode
            });
        }

        // ================= PRODUCT LOOKUP API (FOR DROPDOWN) =================
        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            SetAuthHeader();

            var response = await _httpClient.GetAsync("Product");
            var json = await response.Content.ReadAsStringAsync();

            return Content(json, "application/json");
        }
        public async Task<IActionResult> GetProductsForSales()
        {
            SetAuthHeader();

            var response = await _httpClient.GetAsync(
                "https://localhost:44330/api/Product/lookup-for-sales");

            var json = await response.Content.ReadAsStringAsync();
            return Content(json, "application/json");
        }
    }
}
