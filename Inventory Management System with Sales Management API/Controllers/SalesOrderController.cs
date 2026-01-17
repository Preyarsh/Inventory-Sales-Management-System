using Inventory_Management_System_with_Sales_Management_API.Data;
using Inventory_Management_System_with_Sales_Management_API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Inventory_Management_System_with_Sales_Management_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SalesOrderController : Controller
    {
        private readonly ISalesOrderRepository _repo;

        public SalesOrderController(ISalesOrderRepository repo)
        {
            _repo = repo;
        }
        [HttpPost]
        public IActionResult Create(SalesOrderModel model)
        => Ok(_repo.CreateSalesOrder(model));

        [HttpGet]
        public IActionResult List()
            => Ok(_repo.GetSalesOrders());

        [HttpGet("{id}")]
        public IActionResult Get(int id)
            => Ok(_repo.GetSalesOrderById(id));

        [HttpPut("{id}")]
        public IActionResult Update(int id, SalesOrderModel model)
        {
            model.SalesOrderId = id;
            return Ok(_repo.UpdateSalesOrder(model));
        }

        [HttpPut("{id}/cancel")]
        public IActionResult Cancel(int id, [FromQuery] int userId)
            => Ok(_repo.CancelSalesOrder(id, userId));

        [HttpPost("{id}/convert")]
        [Authorize]
        public IActionResult ConvertToInvoice(int id)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var result = _repo.ConvertToInvoice(id, userId);

            return Ok(result);
        }

    }
}
