using ExpenseWeb.Models;
using ExpenseWeb.Services.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ExpenseWeb.Controllers
{
    public class ExpenseController : Controller
    {
        private readonly IExpenseService _expenseService;
        private readonly ICategoryService _categoryService;

        public ExpenseController(
            IExpenseService expenseService,
            ICategoryService categoryService)
        {
            _expenseService = expenseService;
            _categoryService = categoryService;
        }

        // INDEX COM FILTRO DE MÊS / ANO
        [HttpGet]
        public async Task<IActionResult> Index(int? month, int? year)
        {
            int referenceMonth = month ?? DateTime.Now.Month;
            int referenceYear = year ?? DateTime.Now.Year;

            var expenses = await _expenseService.GetAllExpenses();

            if (expenses is null)
                return View("Error");


            var filteredExpenses = expenses
                .Where(e =>
                    e.ReferenceMonth == referenceMonth &&
                    e.ReferenceYear == referenceYear)
                .ToList();

            ViewBag.SelectedMonth = referenceMonth;
            ViewBag.SelectedYear = referenceYear;

            return View(filteredExpenses);
        }

        // 🔽 CREATE (GET)
        [HttpGet]
        public async Task<IActionResult> CreateExpense()
        {
            ViewBag.CategoryId =
                new SelectList(
                    await _categoryService.GetAllCategories(),
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
                ViewBag.CategoryId =
                    new SelectList(
                        await _categoryService.GetAllCategories(),
                        "CategoryId",
                        "Name");

                return View(expense);
            }

            var result = await _expenseService.CreateExpense(expense);

            if (result is null)
                return View("Error");

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> UpdateExpense(int id)
        {
            var expense = await _expenseService.GetExpenseById(id);

            if (expense is null)
                return NotFound();

            ViewBag.CategoryId =
                new SelectList(
                    await _categoryService.GetAllCategories(),
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
                ViewBag.CategoryId =
                    new SelectList(
                        await _categoryService.GetAllCategories(),
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
        public async Task<IActionResult> DeleteExpenseConfirmed(int expenseId)
        {
            var result = await _expenseService.DeleteExpense(expenseId);

            if (!result)
                return View("Error");

            return RedirectToAction(nameof(Index));

        }
    }
}

