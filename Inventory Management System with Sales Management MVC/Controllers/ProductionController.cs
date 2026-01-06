using ClosedXML.Excel;
using Inventory_Management_System_with_Sales_Management_MVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Data;
using System.Text;

public class ProductionController : Controller
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly string _connectionString;

    public ProductionController(IHttpClientFactory factory, IConfiguration configuration)
    {
        _httpClient = factory.CreateClient();
        _httpClient.BaseAddress = new Uri("https://localhost:44330/api/");
        _configuration = configuration;
        //_connectionString = connectionString;
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

    // ================= LIST =================
    public async Task<IActionResult> ProductionList()
    {
        SetAuthHeader();

        var response = await _httpClient.GetAsync("Production");
        var json = await response.Content.ReadAsStringAsync();

        var list = JsonConvert.DeserializeObject<List<ProductionListViewModel>>(json);

        return View(list);
    }

    // ================= CREATE (GET) =================
    public async Task<IActionResult> AddProduction()
    {
        SetAuthHeader();

        var model = new ProductionModel
        {
            ProductionDate = DateTime.Today,
            Products = await GetProductsDropdown()
        };

        return View(model);
    }

    // ================= CREATE (POST) =================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddProduction(ProductionModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Products = await GetProductsDropdown();
            return View(model);
        }

        model.ProductionDate = DateTime.Today; // ✅ FIX

        SetAuthHeader();

        var json = JsonConvert.SerializeObject(model);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("Production", content);

        if (response.IsSuccessStatusCode)
            return RedirectToAction(nameof(ProductionList));

        ModelState.AddModelError("", "Failed to save production");
        model.Products = await GetProductsDropdown();
        return View(model);
    }
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            SetAuthHeader();

            var response = await _httpClient.DeleteAsync($"Production/{id}");

            if (!response.IsSuccessStatusCode)
            {
                return Json(new
                {
                    success = false,
                    message = "Failed to delete production."
                });
            }

            return Json(new
            {
                success = true,
                message = "Production deleted successfully."
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

    // ================= HELPERS =================
    private async Task<List<SelectListItem>> GetProductsDropdown()
    {
        var response = await _httpClient.GetAsync("Product"); // your Product API
        var json = await response.Content.ReadAsStringAsync();

        var products = JsonConvert.DeserializeObject<List<dynamic>>(json);

        return products.Select(p => new SelectListItem
        {
            Value = p.productId.ToString(),
            Text = p.productName.ToString()
        }).ToList();
    }
    public IActionResult ExportProductsToExcel()
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        DataTable dt = new DataTable();

        using (SqlConnection con = new SqlConnection(connectionString))
        using (SqlCommand cmd = new SqlCommand("PR_Productions_Export", con))
        {
            cmd.CommandType = CommandType.StoredProcedure;
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                da.Fill(dt);
            }
        }

        using (var workbook = new XLWorkbook())
        {
            var sheet = workbook.Worksheets.Add("Productions");
            sheet.Cell(1, 1).InsertTable(dt, "ProductionsTable");
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
                    $"Productions_{DateTime.Now:yyyyMMdd}.xlsx"
                );
            }
        }
    }
}
