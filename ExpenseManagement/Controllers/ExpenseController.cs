using ExpenseManagement.Models;
using ExpenseManagement.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ExpenseManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExpenseController : ControllerBase
    {

        private readonly IExpenseRepository _repository;
        private readonly ILogger<ExpenseController> _logger;
        public ExpenseController(IExpenseRepository repository, ILogger<ExpenseController> logger)
        {
            _repository = repository;
            _logger = logger;
        }


        [HttpGet]
        public ActionResult<IEnumerable<Expense>> Get()
        {

            var expenses = _repository.GetExpenses();
            return Ok(expenses);

        }

        [HttpGet("{id:int}", Name = "ObterExpense")]
        public ActionResult<Category> GetById(int id)
        {
            var category = _repository.GetExpenseId(id);
            if (category == null)
            {
                _logger.LogWarning("Expense with id {ExpenseId} not found.", id);
                return NotFound($"Dívida com id {id} não encontrada...");
            }
            return Ok(category);

        }

        [HttpPost]
        public ActionResult<Category> Post([FromBody] Expense expense)
        {
            if (expense == null)
            {
                _logger.LogWarning("Received null expense object in POST request.");
                return BadRequest("expense inválida.");
            }

            var expenseCriada = _repository.Create(expense);

            return new CreatedAtRouteResult("ObterCategoria", new { id = expenseCriada.ExpenseId }, expenseCriada);
        }

        [HttpPut("{id:int}")]
        public ActionResult Put(int id, Expense expense)
        {
            if (expense == null || expense.ExpenseId != id)
            {
                _logger.LogWarning("Expense ID mismatch or null object in PUT request.");
                return BadRequest("Expense inválida.");
            }
            var expenseExistente = _repository.GetExpenseId(id);
            if (expenseExistente == null)
            {
                _logger.LogWarning("Expense with id {ExpenseId} not found for update.", id);
                return NotFound($"Expense com id {id} não encontrada...");
            }
            _repository.Update(expense);
            return Ok(expense);
        }

        [HttpDelete("{id:int}")]
        public ActionResult Delete(int id)
        {
            var expense = _repository.GetExpenseId(id);

            if (expense == null)
            {
                _logger.LogWarning("Expense with id {ExpenseId} not found for deletion.", id);
                return NotFound($"Expense com id {id} não encontrada...");
            }

            var expenseExcluida = _repository.Delete(id);
            return Ok(expenseExcluida);
        }
    }

}

