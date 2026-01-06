using Inventory_Management_System_with_Sales_Management_API.Data;
using Inventory_Management_System_with_Sales_Management_API.Models;
using Inventory_Management_System_with_Sales_Management_API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventory_Management_System_with_Sales_Management_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // 🔐 JWT Protected
    public class UnitController : ControllerBase
    {
        private readonly UnitRepository _unitRepository;

        public UnitController(UnitRepository unitRepository)
        {
            _unitRepository = unitRepository;
        }

        // ✅ GET: api/Unit?search=&status=true
        [HttpGet]
        public IActionResult GetAll(string? search = null, bool? status = null)
        {
            var units = _unitRepository.GetAll(search, status);
            return Ok(units);
        }

        // ✅ GET: api/Unit/5
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var unit = _unitRepository.GetById(id);

            if (unit == null)
                return NotFound(new { message = "Unit not found" });

            return Ok(unit);
        }

        // ✅ POST: api/Unit
        [HttpPost]
        public IActionResult Insert(UnitModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _unitRepository.Insert(model);
            return Ok(new { message = "Unit added successfully" });
        }

        // ✅ PUT: api/Unit/5
        [HttpPut("{id}")]
        public IActionResult Update(int id, UnitModel model)
        {
            if (id != model.UnitID)
                return BadRequest("Invalid Unit ID");

            _unitRepository.Update(model);
            return Ok(new { message = "Unit updated successfully" });
        }

        // ✅ DELETE: api/Unit/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _unitRepository.Delete(id);
            return Ok(new { message = "Unit deleted successfully" });
        }
        [HttpGet("Dropdown")]
        public IActionResult GetForDropdown()
        {
            return Ok(_unitRepository.GetForDropdown());
        }

    }
}
