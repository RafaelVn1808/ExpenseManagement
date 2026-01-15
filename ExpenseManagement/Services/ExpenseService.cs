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
        private readonly IMapper _mapper;

        public ExpenseService(IExpenseRepository expenseRepository, IMapper mapper)
        {
            _expenseRepository = expenseRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ExpenseDTO>> GetAllExpensesAsync(string userId)
        {
            var expensesEntity = await _expenseRepository.GetExpenses(userId);
            return _mapper.Map<IEnumerable<ExpenseDTO>>(expensesEntity);
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
            // Verificar se a despesa pertence ao usuário
            var existingExpense = await _expenseRepository.GetExpenseId(expenseDto.ExpenseId, userId);
            if (existingExpense == null)
                throw new UnauthorizedAccessException("Despesa não encontrada ou você não tem permissão para editá-la");

            if (expenseDto.Installments <= 0)
                expenseDto.Installments = 1;

            expenseDto.InstallmentAmount =
                Math.Round(expenseDto.TotalAmount / expenseDto.Installments, 2);

            var expenseEntity = _mapper.Map<Expense>(expenseDto);
            expenseEntity.UserId = userId; // Garantir que o userId não seja alterado

            await _expenseRepository.Update(expenseEntity);
        }

        public async Task<ExpenseDTO?> DeleteExpenseAsync(int id, string userId)
        {
            var expense = await _expenseRepository.Delete(id, userId);

            if (expense == null)
                return null;

            return _mapper.Map<ExpenseDTO>(expense);
        }
    }
}
