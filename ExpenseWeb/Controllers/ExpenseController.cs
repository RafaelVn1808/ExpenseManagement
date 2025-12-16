using ExpenseWeb.Models;
using ExpenseWeb.Services.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseWeb.Controllers
{
    public class ExpenseController : Controller
    {
        private readonly IExpenseService _expenseService;

        public ExpenseController(IExpenseService expenseService)
        {
            _expenseService = expenseService;
        }

        public async Task<ActionResult<IEnumerable<ExpenseViewModel>>> Index()
        {
            var result = await _expenseService.GetAllExpenses();

            if (result is null) { 
                return View("Error");
            }

            return View(result);
        }
    }
}
