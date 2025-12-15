using ExpenseManagement.DTOs;
using ExpenseManagement.Models;
using ExpenseManagement.Repositories;
using ExpenseManagement.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace ExpenseManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExpenseController : ControllerBase
    {

        private readonly IExpenseService _service;
        
        public ExpenseController(IExpenseService service)
        {
            _service = service;
            
        }


        [HttpGet]
        public ActionResult<IEnumerable<Expense>> Get()
        {

            var expenses = _service.GetAllExpensesAsync();
            if (expenses == null)
            {
                return NotFound("Nenhuma despesa encontrada...");
            }
            return Ok(expenses);

        }

        [HttpGet("{id:int}", Name = "ObterExpense")]
        public ActionResult<Category> GetById(int id)
        {
            var category = _service.GetExpensesByIdAsync(id);
            if (category == null)
            {
                 return NotFound($"Dívida com id {id} não encontrada...");
            }
            return Ok(category);

        }

        [HttpPost]

        public async Task<ActionResult<Category>> Post([FromBody] ExpenseDTO expenseDTO)
        {
            if (expenseDTO == null)
            {
               return BadRequest("expense inválida.");
            }

            await _service.CreateExpensesAsync(expenseDTO);

            return new CreatedAtRouteResult("ObterCategoria", new { id = expenseDTO.ExpenseId }, expenseDTO);
        }

        [HttpPut("{id:int}")]
        public ActionResult Put(int id, ExpenseDTO expenseDTO)
        {
            if (expenseDTO == null || expenseDTO.ExpenseId != id)
            {
               return BadRequest("Expense inválida.");
            }
            var expenseExistente = _service.GetExpensesByIdAsync(id);
            if (expenseExistente == null)
            {               
                return NotFound($"Expense com id {id} não encontrada...");
            }
            _service.UpdateExpenseAsync(expenseDTO);
            return Ok(expenseDTO);
        }

        [HttpDelete("{id:int}")]
        public ActionResult Delete(int id)
        {
            var expense = _service.GetExpensesByIdAsync(id);

            if (expense == null)
            {
                return NotFound($"Expense com id {id} não encontrada...");
            }

            var expenseExcluida = _service.DeleteExpenseAsync(id);
            return Ok(expenseExcluida);
        }
    }

}

