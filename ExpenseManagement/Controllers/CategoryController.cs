using ExpenseApi.Identity;
using ExpenseManagement.DTOs;
using ExpenseManagement.Models;
using ExpenseManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

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
            if (categoriesDTO == null || !categoriesDTO.Any())
            {
                return NotFound("Nenhuma categoria encontrada.");
            }
            return Ok(categoriesDTO);
        }

        [HttpGet("{id:int}", Name = "GetCategory")]
        public async Task<ActionResult<CategoryDTO>> GetCategoriesById(int id)
        {
            var categoryDTO = await _categoryService.GetCategoryById(id);
            if (categoryDTO == null)
            {
                return NotFound($"Categoria com id {id} não encontrada.");
            }
            return Ok(categoryDTO);
        }

        [HttpPost]
        public async Task<ActionResult<CategoryDTO>> Post([FromBody] CategoryDTO categoryDTO)
        {
            if (categoryDTO == null)
            {
                return BadRequest("Categoria inválida.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _categoryService.AddCategory(categoryDTO);

            return new CreatedAtRouteResult("GetCategory", new { id = categoryDTO.CategoryId }, categoryDTO);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<CategoryDTO>> Put(int id, [FromBody] CategoryDTO categoryDTO)
        {
            if (categoryDTO == null || id != categoryDTO.CategoryId)
            {
                return BadRequest("Categoria inválida.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var categoriaExistente = await _categoryService.GetCategoryById(id);
            if (categoriaExistente == null)
            {
                return NotFound($"Categoria com id {id} não encontrada.");
            }

            await _categoryService.UpdateCategory(categoryDTO);
            return Ok(categoryDTO);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var categoryDTO = await _categoryService.GetCategoryById(id);
            if (categoryDTO == null)
            {
                return NotFound($"Categoria com id {id} não encontrada.");
            }

            await _categoryService.RemoveCategory(id);

            return NoContent();
        }
    }
}
