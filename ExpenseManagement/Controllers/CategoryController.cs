using ExpenseApi.Identity;
using ExpenseManagement.DTOs;
using ExpenseManagement.Models;
using ExpenseManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseManagement.Controllers
{
    [Authorize(Roles = Roles.Admin)]
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {

        private readonly ICategoryService _categoryService;




        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;

        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryDTO>>> GetCategories()
        {
            var categoriesDTO = await _categoryService.GetCategories();
            if (categoriesDTO == null)
            {
                return NotFound();
            }
            return Ok(categoriesDTO);
        }

        [HttpGet("{id:int}", Name = "GetCategory")]
        public async Task<ActionResult<CategoryDTO>> GetCategoriesById(int id)
        {
            var categoriesDTO = await _categoryService.GetCategoryById(id);
            if (categoriesDTO == null)
            {
                return NotFound();
            }
            return Ok(categoriesDTO);
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] CategoryDTO categoryDTO)
        {
            if (categoryDTO == null)
            {
                return BadRequest("Invalid Data");
            }

            await _categoryService.AddCategory(categoryDTO);

            return new CreatedAtRouteResult("GetCategory", new { id = categoryDTO.CategoryId }, categoryDTO);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, [FromBody] CategoryDTO categoryDTO)
        {
            if (id != categoryDTO.CategoryId)
            {
                return BadRequest("Invalid Data");
            }

            if (categoryDTO == null)
            {
                return BadRequest("Invalid Data");
            }
            await _categoryService.UpdateCategory(categoryDTO);
            return Ok(categoryDTO);
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var categoryDTO = await _categoryService.GetCategoryById(id);
            if (categoryDTO == null)
            {
                return NotFound("Product not found");
            }
            await _categoryService.RemoveCategory(id);

            return Ok(categoryDTO);

        }
    }
}
