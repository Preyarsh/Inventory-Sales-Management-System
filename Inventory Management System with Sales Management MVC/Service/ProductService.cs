using Inventory_Management_System_with_Sales_Management_MVC.Models;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace Inventory_Management_System_with_Sales_Management_MVC.Service
{
    public class ProductService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProductService(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        // ✅ Centralized method to set JWT header
        private void SetAuthHeader()
        {
            var token = _httpContextAccessor.HttpContext?
                .Session.GetString("JWToken");

            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<List<ProductFormViewModel>> GetAllProductsAsync()
        {
            // ✅ Attach JWT before calling API
            SetAuthHeader();

            var response = await _httpClient.GetAsync("api/Product");

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<List<ProductFormViewModel>>(json);
        }
    }
}
