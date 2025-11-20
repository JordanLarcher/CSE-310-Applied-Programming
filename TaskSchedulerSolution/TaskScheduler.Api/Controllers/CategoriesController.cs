using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskScheduler.Application.Services.Interfaces;
using TaskScheduler.Domain.Entities;

namespace TaskScheduler.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim ?? "0");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
        {
            var userId = GetCurrentUserId();
            var categories = await _categoryService.GetUserCategoriesAsync(userId);
            return Ok(categories);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> GetCategory(int id)
        {
            var userId = GetCurrentUserId();
            var category = await _categoryService.GetCategoryByIdAsync(userId, id);

            if (category == null)
            {
                return NotFound(new { message = "Category not found" });
            }

            return Ok(category);
        }

        [HttpPost]
        public async Task<ActionResult<Category>> CreateCategory([FromBody] CreateCategoryDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var category = await _categoryService.CreateCategoryAsync(userId, dto.Name, dto.Color);
                return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Category>> UpdateCategory(int id, [FromBody] UpdateCategoryDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var category = await _categoryService.UpdateCategoryAsync(userId, id, dto.Name, dto.Color);
                return Ok(category);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteCategory(int id)
        {
            var userId = GetCurrentUserId();
            var result = await _categoryService.DeleteCategoryAsync(userId, id);

            if (result)
            {
                return Ok(new { message = "Category deleted successfully" });
            }

            return NotFound(new { message = "Category not found" });
        }
    }

    public class CreateCategoryDto
    {
        public string Name { get; set; } = string.Empty;
        public string Color { get; set; } = "#000000";
    }

    public class UpdateCategoryDto
    {
        public string Name { get; set; } = string.Empty;
        public string Color { get; set; } = "#000000";
    }
}
