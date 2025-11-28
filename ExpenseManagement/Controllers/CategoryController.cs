using ExpenseManagement.Models;
using ExpenseManagement.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {

        private readonly ICategoryRepository _repository;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(ICategoryRepository repository, ILogger<CategoryController> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        [HttpGet]
        public ActionResult<IEnumerable<Category>> Get()
        {

            var categories = _repository.GetCategories();
            return Ok(categories);

        }

        [HttpGet("{id:int}", Name = "ObterCategoria")]
        public ActionResult<Category> GetById(int id)
        {
            var category = _repository.GetCategoryById(id);
            if (category == null)
            {
                _logger.LogWarning("Category with id {CategoryId} not found.", id);
                return NotFound($"Categoria com id {id} não encontrada...");
            }
            return Ok(category);

        }

        [HttpPost]
        public ActionResult<Category> Post([FromBody] Category category)
        {
            if (category == null)
            {
                _logger.LogWarning("Received null category object in POST request.");
                return BadRequest("Categoria inválida.");
            }

            var categoriaCriada = _repository.Create(category);

            return new CreatedAtRouteResult("ObterCategoria", new { id = categoriaCriada.CategoryId }, categoriaCriada);
        }

        [HttpPut("{id:int}")]
        public ActionResult Put(int id, Category category)
        {
            if (category == null || category.CategoryId != id)
            {
                _logger.LogWarning("Category ID mismatch or null object in PUT request.");
                return BadRequest("Categoria inválida.");
            }
            var categoriaExistente = _repository.GetCategoryById(id);
            if (categoriaExistente == null)
            {
                _logger.LogWarning("Category with id {CategoryId} not found for update.", id);
                return NotFound($"Categoria com id {id} não encontrada...");
            }
            _repository.Update(category);
            return Ok(category);
        }
    }
}
