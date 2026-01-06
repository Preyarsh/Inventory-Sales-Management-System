using Inventory_Management_System_with_Sales_Management_API.Data;
using Inventory_Management_System_with_Sales_Management_API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventory_Management_System_with_Sales_Management_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductController : ControllerBase
    {
        private readonly ProductRepository _productRepository;

        public ProductController(ProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        // 🔹 GET: api/Product
        [HttpGet]
        public IActionResult GetAll()
        {
            var data = _productRepository.GetAll();
            return Ok(data);
        }

        // 🔹 GET: api/Product/5
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var product = _productRepository.GetById(id);
            if (product == null)
                return NotFound();

            return Ok(product);
        }

        // 🔹 POST: api/Product
        [HttpPost]
        public IActionResult Insert([FromBody] ProductModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = _productRepository.Insert(model);

            if (result)
                return Ok(new { message = "Product inserted successfully" });

            return BadRequest("Failed to insert product");
        }

        // 🔹 PUT: api/Product
        [HttpPut]
        public IActionResult Update([FromBody] ProductModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = _productRepository.Update(model);

            if (result)
                return Ok(new { message = "Product updated successfully" });

            return BadRequest("Failed to update product");
        }

        // 🔹 DELETE: api/Product/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var result = _productRepository.Delete(id);

            if (result)
                return Ok(new { message = "Product deleted successfully" });

            return BadRequest("Failed to delete product");
        }
    }
}
