using ExpenseApi.Identity;
using ExpenseManagement.DTOs;
using ExpenseManagement.Models;
using ExpenseManagement.Repositories;
using ExpenseManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseManagement.Controllers
{
    [Authorize]
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
        public async Task<ActionResult<IEnumerable<ExpenseDTO>>> Get()
        {
            var expenses = await _service.GetAllExpensesAsync();
            if (expenses == null || !expenses.Any())
            {
                return NotFound("Nenhuma despesa encontrada.");
            }
            return Ok(expenses);
        }

        [HttpGet("{id:int}", Name = "ObterExpense")]
        public async Task<ActionResult<ExpenseDTO>> GetById(int id)
        {
            var expense = await _service.GetExpensesByIdAsync(id);
            if (expense == null)
            {
                return NotFound($"Despesa com id {id} não encontrada.");
            }
            return Ok(expense);
        }

        [HttpPost]
        public async Task<ActionResult<ExpenseDTO>> Post([FromBody] ExpenseDTO expenseDTO)
        {
            if (expenseDTO == null)
            {
                return BadRequest("Despesa inválida.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _service.CreateExpensesAsync(expenseDTO);

            return new CreatedAtRouteResult("ObterExpense", new { id = expenseDTO.ExpenseId }, expenseDTO);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<ExpenseDTO>> Put(int id, ExpenseDTO expenseDTO)
        {
            if (expenseDTO == null || expenseDTO.ExpenseId != id)
            {
                return BadRequest("Despesa inválida.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var expenseExistente = await _service.GetExpensesByIdAsync(id);
            if (expenseExistente == null)
            {
                return NotFound($"Despesa com id {id} não encontrada.");
            }

            await _service.UpdateExpenseAsync(expenseDTO);
            return Ok(expenseDTO);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var expense = await _service.GetExpensesByIdAsync(id);

            if (expense == null)
                return NotFound($"Despesa com id {id} não encontrada.");

            await _service.DeleteExpenseAsync(id);

            return NoContent();
        }

    }

}

