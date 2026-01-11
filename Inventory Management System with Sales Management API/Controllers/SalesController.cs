using Inventory_Management_System_with_Sales_Management_API.Data;
using Inventory_Management_System_with_Sales_Management_API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Security.Claims;

namespace Inventory_Management_System_with_Sales_Management_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SalesController : Controller
    {
        private readonly ISalesRepository _repository;

        public SalesController(ISalesRepository repository)
        {
            _repository = repository;
        }
        [HttpPost]
        public IActionResult CreateSale([FromBody] SaleCreateModel model)
        {
            model.CreatedBy = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            if (!ModelState.IsValid || model.Items.Count == 0)
                return BadRequest("Invalid sale data");

            var saleId = _repository.CreateSale(model);

            return Ok(new
            {
                SaleId = saleId,
                Message = "Sale created successfully"
            });
        }

        [HttpGet]
        public IActionResult GetSales()
        {
            return Ok(_repository.GetSales());
        }

        [HttpGet("{saleId}")]
        public IActionResult GetInvoice(int saleId)
        {
            return Ok(_repository.GetInvoice(saleId));
        }
    }
}
