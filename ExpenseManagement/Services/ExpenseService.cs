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

            await _expenseRepository.Update(existingExpense);
        }

        public async Task<ExpenseDTO?> DeleteExpenseAsync(int id, string userId)
        {
            var expense = await _expenseRepository.Delete(id, userId);

            if (expense == null)
                return null;

            return _mapper.Map<ExpenseDTO>(expense);
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
