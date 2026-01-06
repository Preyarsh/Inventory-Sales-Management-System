using Inventory_Management_System_with_Sales_Management_API.Data;
using Inventory_Management_System_with_Sales_Management_API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventory_Management_System_with_Sales_Management_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CategoryController : Controller
    {
        private readonly CategoryRepository _categoryRepository;
        private readonly IConfiguration _config;

        public CategoryController(CategoryRepository categoryRepository, IConfiguration config)
        {
            _categoryRepository = categoryRepository;
            _config = config;
        }
        [HttpGet("Category")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetAllUsers()
        {
            var category = _categoryRepository.GetAll();
            return Ok(category);
        }
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetById(int id)
        {
            var category = _categoryRepository.GetById(id);

            if (category == null)
            {
                return NotFound(new { message = "Category not found" });
            }

            return Ok(category);
        }
        [HttpPost]
        public IActionResult Insert(CategoryModel model)
        {
            _categoryRepository.Insert(model);
            return Ok(new { message = "Category added successfully" });
        }
        [HttpPut("{id}")]
        public IActionResult Update(int id, CategoryModel model)
        {
            if (id != model.CategoryId)
                return BadRequest("Category ID mismatch");

            _categoryRepository.Update(model);
            return Ok(new { message = "Category updated successfully" });
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _categoryRepository.Delete(id);
            return Ok(new { message = "Category deleted successfully" });
        }
        [HttpGet("Dropdown")]
        public IActionResult GetForDropdown()
        {
            return Ok(_categoryRepository.GetForDropdown());
        }

    }
}
