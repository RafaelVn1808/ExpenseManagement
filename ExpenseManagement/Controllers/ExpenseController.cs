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
using System.Security.Claims;
using System.Threading.Tasks;

namespace ExpenseManagement.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ExpenseController : ControllerBase
    {
        private readonly IExpenseService _service;
        private readonly ILogger<ExpenseController> _logger;

        public ExpenseController(IExpenseService service, ILogger<ExpenseController> logger)
        {
            _service = service;
            _logger = logger;
        }

        // Helper method para obter o UserId do token JWT
        private string GetUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("Usuário não autenticado.");
            }
            return userId;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ExpenseDTO>>> Get()
        {
            var userId = GetUserId();
            var expenses = await _service.GetAllExpensesAsync(userId);
            
            if (expenses == null || !expenses.Any())
            {
                return NotFound("Nenhuma despesa encontrada.");
            }
            
            return Ok(expenses);
        }

        [HttpGet("{id:int}", Name = "ObterExpense")]
        public async Task<ActionResult<ExpenseDTO>> GetById(int id)
        {
            var userId = GetUserId();
            var expense = await _service.GetExpensesByIdAsync(id, userId);
            
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

            var userId = GetUserId();
            
            try
            {
                await _service.CreateExpensesAsync(expenseDTO, userId);
                
                // Buscar a despesa criada para retornar com o ID correto
                var createdExpenses = await _service.GetAllExpensesAsync(userId);
                var createdExpense = createdExpenses.OrderByDescending(e => e.ExpenseId).FirstOrDefault();
                
                if (createdExpense != null)
                {
                    return new CreatedAtRouteResult("ObterExpense", new { id = createdExpense.ExpenseId }, createdExpense);
                }
                
                return CreatedAtAction(nameof(GetById), new { id = expenseDTO.ExpenseId }, expenseDTO);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar despesa para o usuário {UserId}", userId);
                return BadRequest($"Erro ao criar despesa: {ex.Message}");
            }
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

            var userId = GetUserId();

            try
            {
                await _service.UpdateExpenseAsync(expenseDTO, userId);
                return Ok(expenseDTO);
            }
            catch (UnauthorizedAccessException)
            {
                return NotFound($"Despesa com id {id} não encontrada ou você não tem permissão para editá-la.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar despesa {ExpenseId} para o usuário {UserId}", id, userId);
                return BadRequest($"Erro ao atualizar despesa: {ex.Message}");
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetUserId();

            try
            {
                var expense = await _service.DeleteExpenseAsync(id, userId);

                if (expense == null)
                    return NotFound($"Despesa com id {id} não encontrada.");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao deletar despesa {ExpenseId} para o usuário {UserId}", id, userId);
                return BadRequest($"Erro ao deletar despesa: {ex.Message}");
            }
        }
    }
}

