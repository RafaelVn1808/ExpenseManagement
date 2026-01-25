using ExpenseWeb.Models;
using ExpenseWeb.Services;
using ExpenseWeb.Services.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;

namespace ExpenseWeb.Controllers
{
    [Authorize]
    public class ExpenseController : Controller
    {
        private readonly IExpenseService _expenseService;
        private readonly ICategoryService _categoryService;
        private readonly ImageUploadService _imageUploadService;

        public ExpenseController(
            IExpenseService expenseService,
            ICategoryService categoryService,
            ImageUploadService imageUploadService)
        {
            _expenseService = expenseService;
            _categoryService = categoryService;
            _imageUploadService = imageUploadService;
        }

        // INDEX COM FILTRO DE MÊS / ANO
        [HttpGet]
        public async Task<IActionResult> Index(int? month, int? year, int page = 1, int pageSize = 20)
        {
            int referenceMonth = month ?? DateTime.Now.Month;
            int referenceYear = year ?? DateTime.Now.Year;

            var firstDay = new DateTime(referenceYear, referenceMonth, 1);
            var lastDay = firstDay.AddMonths(1).AddDays(-1);

            var parameters = new ExpenseQueryParameters
            {
                Page = page,
                PageSize = pageSize,
                From = firstDay,
                To = lastDay,
                SortBy = "startDate",
                SortDir = "desc"
            };

            var pagedExpenses = await _expenseService.GetExpensesPaged(parameters);

            ViewBag.SelectedMonth = referenceMonth;
            ViewBag.SelectedYear = referenceYear;
            ViewBag.PageSize = pageSize;

            return View(pagedExpenses);
        }

        // 🔽 CREATE (GET)
        [HttpGet]
        public async Task<IActionResult> CreateExpense()
        {
            var categories = await _categoryService.GetAllCategories();
            
            ViewBag.CategoryId =
                new SelectList(
                    categories ?? Enumerable.Empty<CategoryViewModel>(),
                    "CategoryId",
                    "Name");

            return View();
        }

        // 🔽 CREATE (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateExpense(ExpenseViewModel expense)
        {
            if (!ModelState.IsValid)
            {
                var categories = await _categoryService.GetAllCategories();
                
                ViewBag.CategoryId =
                    new SelectList(
                        categories ?? Enumerable.Empty<CategoryViewModel>(),
                        "CategoryId",
                        "Name");

                return View(expense);
            }

            // Processar upload de imagens
            try
            {
                if (expense.NoteImageFile != null)
                {
                    expense.NoteImageUrl = await _imageUploadService.UploadImageAsync(expense.NoteImageFile, "note");
                }

                if (expense.ProofImageFile != null)
                {
                    expense.ProofImageUrl = await _imageUploadService.UploadImageAsync(expense.ProofImageFile, "proof");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                var categories = await _categoryService.GetAllCategories();
                ViewBag.CategoryId = new SelectList(
                    categories ?? Enumerable.Empty<CategoryViewModel>(),
                    "CategoryId",
                    "Name");
                return View(expense);
            }

            try
            {
                var result = await _expenseService.CreateExpense(expense);

                if (result is null)
                {
                    ModelState.AddModelError("", "Erro ao criar despesa. Tente novamente.");
                    var categories = await _categoryService.GetAllCategories();
                    ViewBag.CategoryId = new SelectList(
                        categories ?? Enumerable.Empty<CategoryViewModel>(),
                        "CategoryId",
                        "Name");
                    return View(expense);
                }

                TempData["SuccessMessage"] = "Despesa cadastrada com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                var categories = await _categoryService.GetAllCategories();
                ViewBag.CategoryId = new SelectList(
                    categories ?? Enumerable.Empty<CategoryViewModel>(),
                    "CategoryId",
                    "Name");
                return View(expense);
            }
        }

        [HttpGet]
        public async Task<IActionResult> UpdateExpense(int id)
        {
            var expense = await _expenseService.GetExpenseById(id);

            if (expense is null)
                return NotFound();

            var categories = await _categoryService.GetAllCategories();
            
            ViewBag.CategoryId =
                new SelectList(
                    categories ?? Enumerable.Empty<CategoryViewModel>(),
                    "CategoryId",
                    "Name",
                    expense.CategoryId);

            return View(expense);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateExpense(ExpenseViewModel expense)
        {
            if (!ModelState.IsValid)
            {
                var categories = await _categoryService.GetAllCategories();
                
                ViewBag.CategoryId =
                    new SelectList(
                        categories ?? Enumerable.Empty<CategoryViewModel>(),
                        "CategoryId",
                        "Name",
                        expense.CategoryId);

                return View(expense);
            }

            // Buscar despesa existente para manter URLs antigas se não houver novo upload
            var existingExpense = await _expenseService.GetExpenseById(expense.ExpenseId);
            if (existingExpense != null)
            {
                expense.NoteImageUrl = existingExpense.NoteImageUrl;
                expense.ProofImageUrl = existingExpense.ProofImageUrl;
            }

            // Processar upload de novas imagens
            try
            {
                if (expense.NoteImageFile != null)
                {
                    // Deletar imagem antiga se existir
                    if (!string.IsNullOrEmpty(existingExpense?.NoteImageUrl))
                    {
                        await _imageUploadService.DeleteImageAsync(existingExpense.NoteImageUrl);
                    }
                    expense.NoteImageUrl = await _imageUploadService.UploadImageAsync(expense.NoteImageFile, "note");
                }
                else if (expense.RemoveNoteImage)
                {
                    if (!string.IsNullOrEmpty(existingExpense?.NoteImageUrl))
                    {
                        await _imageUploadService.DeleteImageAsync(existingExpense.NoteImageUrl);
                    }
                    expense.NoteImageUrl = null;
                }

                if (expense.ProofImageFile != null)
                {
                    // Deletar imagem antiga se existir
                    if (!string.IsNullOrEmpty(existingExpense?.ProofImageUrl))
                    {
                        await _imageUploadService.DeleteImageAsync(existingExpense.ProofImageUrl);
                    }
                    expense.ProofImageUrl = await _imageUploadService.UploadImageAsync(expense.ProofImageFile, "proof");
                }
                else if (expense.RemoveProofImage)
                {
                    if (!string.IsNullOrEmpty(existingExpense?.ProofImageUrl))
                    {
                        await _imageUploadService.DeleteImageAsync(existingExpense.ProofImageUrl);
                    }
                    expense.ProofImageUrl = null;
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                var categories = await _categoryService.GetAllCategories();
                ViewBag.CategoryId = new SelectList(
                    categories ?? Enumerable.Empty<CategoryViewModel>(),
                    "CategoryId",
                    "Name",
                    expense.CategoryId);
                return View(expense);
            }

            var updatedExpense = await _expenseService.UpdateExpense(expense);

            if (updatedExpense is null)
                return View("Error");

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> DeleteExpense(int id)
        {
            var expense = await _expenseService.GetExpenseById(id);

            if (expense is null)
                return NotFound();

            return View(expense);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteExpenseConfirmed(int expenseId)
        {
            var result = await _expenseService.DeleteExpense(expenseId);

            if (!result)
                return View("Error");

            return RedirectToAction(nameof(Index));

        }
    }
}

