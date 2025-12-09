using ExpenseManagement.DTOs;
using ExpenseManagement.Models;
using ExpenseManagement.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {

        private readonly ICategoryService _categoryService;
       

        

        public CategoryController(ICategoryService categoryService )
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

        [HttpGet]
        public async Task<ActionResult<CategoryDTO>> GetCategorieById(int id) 
        {
            var categoriesDTO = await _categoryService.GetCategoryById(id);

            return
        }


    }
}
