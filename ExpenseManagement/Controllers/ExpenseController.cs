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
        private readonly IImageUploadService _imageUploadService;
        private readonly ILogger<ExpenseController> _logger;
        private readonly IConfiguration _configuration;

        public ExpenseController(
            IExpenseService service,
            IImageUploadService imageUploadService,
            ILogger<ExpenseController> logger,
            IConfiguration configuration)
        {
            _service = service;
            _imageUploadService = imageUploadService;
            _logger = logger;
            _configuration = configuration;
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
        public async Task<ActionResult<PagedResult<ExpenseDTO>>> Get([FromQuery] ExpenseQueryParameters parameters)
        {
            var userId = GetUserId();
            var expenses = await _service.GetExpensesPagedAsync(parameters, userId);

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

        [HttpPost("upload")]
        [RequestSizeLimit(5 * 1024 * 1024)]
        public async Task<IActionResult> UploadImage([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Arquivo inválido.");
            }

            var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp", "image/gif" };
            if (!allowedTypes.Contains(file.ContentType))
            {
                return BadRequest("Tipo de arquivo não suportado. Use JPEG, PNG ou WEBP.");
            }

            var relativeUrl = await _imageUploadService.SaveExpenseImageAsync(file);
            var baseUrl = _configuration["PublicBaseUrl"];
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                baseUrl = $"{Request.Scheme}://{Request.Host}";
            }
            else
            {
                baseUrl = baseUrl.TrimEnd('/');
            }
            var absoluteUrl = $"{baseUrl}{relativeUrl}";

            return Ok(new { url = absoluteUrl });
        }

        [HttpPost("delete-image")]
        public async Task<IActionResult> DeleteImage([FromBody] DeleteImageRequestDto request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Url))
            {
                return BadRequest("URL inválida.");
            }

            await _imageUploadService.DeleteExpenseImageAsync(request.Url);
            return Ok();
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

