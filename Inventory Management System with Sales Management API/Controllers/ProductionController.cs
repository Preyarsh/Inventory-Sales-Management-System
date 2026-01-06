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
    public class ProductionController : Controller
    {
        
        
            private readonly ProductionRepository _repository;

            public ProductionController(ProductionRepository repository)
            {
                _repository = repository;
            }

            // ================= CREATE PRODUCTION =================
            [HttpPost]
            public IActionResult Create([FromBody] ProductionModel model)
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                int userId = GetLoggedInUserId();
                model.CreatedBy = userId;

                int productionId = _repository.Insert(model);

                return Ok(new
                {
                    success = true,
                    message = "Production added successfully",
                    productionId
                });
            }

            // ================= UPDATE PRODUCTION =================
            [HttpPut("{id}")]
            public IActionResult Update(int id, [FromBody] ProductionModel model)
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                int userId = GetLoggedInUserId();

                _repository.Update(id, model, userId);

                return Ok(new
                {
                    success = true,
                    message = "Production updated successfully"
                });
            }

            // ================= DELETE PRODUCTION (SOFT DELETE) =================
            [HttpDelete("{id}")]
            public IActionResult Delete(int id)
            {
                int userId = GetLoggedInUserId();

                _repository.Delete(id, userId);

                return Ok(new
                {
                    success = true,
                    message = "Production deleted successfully"
                });
            }

        // ================= GET ALL PRODUCTIONS =================
        [HttpGet]
        public IActionResult GetAll()
        {
            var productions = _repository.GetAll();
            return Ok(productions);
        }

        // ================= GET PRODUCTION BY ID =================
        [HttpGet("{id}")]
            public IActionResult GetById(int id)
            {
                var production = _repository.GetById(id);

                if (production == null)
                    return NotFound(new { message = "Production not found" });

                return Ok(production);
            }

            // ================= HELPER METHOD =================
            private int GetLoggedInUserId()
            {
                return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            }
        }
    }

