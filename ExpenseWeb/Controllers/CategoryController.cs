using ExpenseWeb.Models;
using ExpenseWeb.Services.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseWeb.Controllers
{
    [Authorize]
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var categories = await _categoryService.GetAllCategories();
            return View(categories.ToList());
        }

        // GET: Category/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Category/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryViewModel category)
        {
            if (!ModelState.IsValid)
            {
                return View(category);
            }

            try
            {
                var result = await _categoryService.CreateCategory(category);

                if (result is null)
                {
                    ModelState.AddModelError("", "Erro ao criar categoria. Verifique se está logado e se a API está acessível e tente novamente.");
                    return View(category);
                }

                TempData["SuccessMessage"] = "Categoria criada com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            catch (UnauthorizedAccessException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(category);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(category);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Erro ao criar categoria: {ex.Message}");
                return View(category);
            }
        }

        /// <summary>
        /// Cria categoria via AJAX (ex.: modal na tela de Nova Despesa). Retorna JSON com id e nome.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateAjax([FromBody] CategoryViewModel category)
        {
            if (category == null || string.IsNullOrWhiteSpace(category.Name))
            {
                return Json(new { success = false, message = "Nome da categoria é obrigatório." });
            }

            try
            {
                var result = await _categoryService.CreateCategory(category);
                if (result is null)
                {
                    return Json(new { success = false, message = "Erro ao criar categoria." });
                }
                return Json(new { success = true, categoryId = result.CategoryId, name = result.Name });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _categoryService.DeleteCategory(id);
            TempData[success ? "SuccessMessage" : "ErrorMessage"] =
                success ? "Categoria excluída com sucesso." : "Não foi possível excluir a categoria (pode estar em uso por despesas).";
            return RedirectToAction(nameof(Index));
        }
    }
}
