using AutoMapper;
using ExpenseApi.ErrorResponse;
using ExpenseManagement.DTOs;
using ExpenseManagement.Models;
using ExpenseManagement.Repositories;

namespace ExpenseManagement.Services
{
    public class ExpenseService : IExpenseService
    {
        private readonly IExpenseRepository _expenseRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public ExpenseService(IExpenseRepository expenseRepository, ICategoryRepository categoryRepository, IMapper mapper)
        {
            _expenseRepository = expenseRepository;
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ExpenseDTO>> GetAllExpensesAsync(string userId)
        {
            var expensesEntity = await _expenseRepository.GetExpenses(userId);
            return _mapper.Map<IEnumerable<ExpenseDTO>>(expensesEntity);
        }

        public async Task<PagedResult<ExpenseDTO>> GetExpensesPagedAsync(ExpenseQueryParameters parameters, string userId)
        {
            if (parameters.Page <= 0)
                parameters.Page = 1;

            if (parameters.PageSize <= 0)
                parameters.PageSize = 20;

            if (parameters.PageSize > 100)
                parameters.PageSize = 100;

            if (parameters.From.HasValue && parameters.To.HasValue && parameters.From > parameters.To)
                throw new BusinessException("A data inicial não pode ser maior que a data final.");

            var pagedExpenses = await _expenseRepository.GetExpensesPaged(parameters, userId);

            return new PagedResult<ExpenseDTO>
            {
                Page = pagedExpenses.Page,
                PageSize = pagedExpenses.PageSize,
                TotalCount = pagedExpenses.TotalCount,
                Items = _mapper.Map<IEnumerable<ExpenseDTO>>(pagedExpenses.Items)
            };
        }

        public async Task<ExpenseDTO?> GetExpensesByIdAsync(int id, string userId)
        {
            var expenseEntity = await _expenseRepository.GetExpenseId(id, userId);

            if (expenseEntity == null)
                return null;

            return _mapper.Map<ExpenseDTO>(expenseEntity);
        }

        public async Task CreateExpensesAsync(ExpenseDTO expenseDto, string userId)
        {
            await ValidateExpenseAsync(expenseDto);

            if (expenseDto.TotalAmount <= 0)
                throw new BusinessException("O valor total deve ser maior que zero");

            if (expenseDto.Installments <= 0)
                expenseDto.Installments = 1;

            expenseDto.InstallmentAmount =
                Math.Round(expenseDto.TotalAmount / expenseDto.Installments, 2);

            var expenseEntity = _mapper.Map<Expense>(expenseDto);
            expenseEntity.UserId = userId; // Garantir que o userId seja do usuário autenticado
            expenseEntity.CreatedAt = DateTime.UtcNow;

            await _expenseRepository.Create(expenseEntity);
        }

        public async Task UpdateExpenseAsync(ExpenseDTO expenseDto, string userId)
        {
            await ValidateExpenseAsync(expenseDto);

            // Verificar se a despesa pertence ao usuário
            var existingExpense = await _expenseRepository.GetExpenseId(expenseDto.ExpenseId, userId);
            if (existingExpense == null)
                throw new UnauthorizedAccessException("Despesa não encontrada ou você não tem permissão para editá-la");

            if (expenseDto.Installments <= 0)
                expenseDto.Installments = 1;

            expenseDto.InstallmentAmount =
                Math.Round(expenseDto.TotalAmount / expenseDto.Installments, 2);

            _mapper.Map(expenseDto, existingExpense);
            existingExpense.UserId = userId; // Garantir que o userId não seja alterado
            existingExpense.UpdatedAt = DateTime.UtcNow;

            await _expenseRepository.Update(existingExpense);
        }

        public async Task<ExpenseDTO?> DeleteExpenseAsync(int id, string userId)
        {
            var expense = await _expenseRepository.Delete(id, userId);

            if (expense == null)
                return null;

            return _mapper.Map<ExpenseDTO>(expense);
        }

        public async Task<DashboardStatsDto> GetDashboardStatsAsync(string userId, DateTime from, DateTime to)
        {
            var expenses = (await _expenseRepository.GetExpensesForStats(userId, from, to)).ToList();

            var byCategory = expenses
                .GroupBy(e => new { e.CategoryId, Name = e.Category?.Name ?? "Sem categoria" })
                .Select(g => new CategorySumDto
                {
                    CategoryName = g.Key.Name,
                    Total = g.Sum(e => e.Installments <= 1 ? e.TotalAmount : e.InstallmentAmount)
                })
                .OrderByDescending(x => x.Total)
                .ToList();

            var byMonth = new List<MonthSumDto>();
            var current = new DateTime(from.Year, from.Month, 1);
            var end = new DateTime(to.Year, to.Month, 1);
            var ci = System.Globalization.CultureInfo.CurrentCulture;
            while (current <= end)
            {
                var monthTotal = expenses
                    .Where(e =>
                    {
                        if (e.Installments <= 1)
                            return e.StartDate.Year == current.Year && e.StartDate.Month == current.Month;
                        var diffMonths = (current.Year - e.StartDate.Year) * 12 + (current.Month - e.StartDate.Month);
                        return diffMonths >= 0 && diffMonths < e.Installments;
                    })
                    .Sum(e => e.Installments <= 1 ? e.TotalAmount : e.InstallmentAmount);
                byMonth.Add(new MonthSumDto
                {
                    Year = current.Year,
                    Month = current.Month,
                    Label = $"{ci.DateTimeFormat.GetMonthName(current.Month).Substring(0, 3)}/{current.Year}",
                    Total = monthTotal
                });
                current = current.AddMonths(1);
            }

            return new DashboardStatsDto { ByCategory = byCategory, ByMonth = byMonth };
        }

        private async Task ValidateExpenseAsync(ExpenseDTO expenseDto)
        {
            if (!Enum.IsDefined(typeof(ExpenseStatus), expenseDto.Status))
                throw new BusinessException("O status informado é inválido.");

            var category = await _categoryRepository.GetCategoryById(expenseDto.CategoryId);
            if (category == null)
                throw new BusinessException("A categoria informada não existe.");

            if (expenseDto.StartDate.Date > DateTime.Today)
                throw new BusinessException("A data de início não pode ser futura.");

            if (expenseDto.Validity.HasValue && expenseDto.Validity.Value < expenseDto.StartDate)
                throw new BusinessException("A data de validade não pode ser anterior à data de início.");
        }
    }
}
