using Inventory_Management_System_with_Sales_Management_API.Data;
using Inventory_Management_System_with_Sales_Management_API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Inventory_Management_System_with_Sales_Management_API.Controllers
{
    [ApiController]
    [Route("api/customers")]
    [Authorize]
    public class CustomersController : ControllerBase
    {
        private readonly CustomerRepository _repo;

        public CustomersController(CustomerRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public IActionResult GetAll() => Ok(_repo.GetAll());

        [HttpGet("{id}")]
        public IActionResult GetById(int id) => Ok(_repo.GetById(id));

        [HttpPost]
        public IActionResult Insert(CustomerModel model)
        {
            model.CreatedBy = GetUserIdFromToken();
            return Ok(_repo.Insert(model));
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id,CustomerModel model)
        {
            model.CustomerId = id;
            model.UpdatedBy = GetUserIdFromToken();
            return Ok(_repo.Update(model));
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            return Ok(_repo.Delete(id, 1));
        }
        private int GetUserIdFromToken()
        {
            var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return id == null ? 0 : int.Parse(id);
        }
        //[HttpGet("dropdown")]
        //public IActionResult Dropdown() => Ok(_repo.GetCustomerDropdown());
    }
}
