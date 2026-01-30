using ExpenseWeb.Models;
using ExpenseWeb.Services.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ExpenseWeb.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private readonly IExpenseService _expenseService;

        public HomeController(IExpenseService expenseService)
        {
            _expenseService = expenseService;
        }

        public async Task<IActionResult> Index()
        {
            DashboardStatsViewModel? stats = null;
            if (User.Identity?.IsAuthenticated == true)
            {
                var to = DateTime.Now.Date;
                var from = to.AddMonths(-11);
                stats = await _expenseService.GetDashboardStatsAsync(from, to);
            }
            return View(stats);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
