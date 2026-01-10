using AutoMapper;
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

        public async Task<IEnumerable<ExpenseDTO>> GetAllExpensesAsync()
        {
            var expensesEntity = await _expenseRepository.GetExpenses();
            return _mapper.Map<IEnumerable<ExpenseDTO>>(expensesEntity);
        }

        public async Task<ExpenseDTO> GetExpensesByIdAsync(int id)
        {
            var expensesEntity = await _expenseRepository.GetExpenseId(id);
            return _mapper.Map<ExpenseDTO>(expensesEntity);
            
        }

        public async Task CreateExpensesAsync(ExpenseDTO expenseDto)
        {
            // Regra de negócio
            if (expenseDto.Installments <= 0)
                expenseDto.Installments = 1;

            expenseDto.InstallmentAmount =
                Math.Round(expenseDto.TotalAmount / expenseDto.Installments, 2);

            var expenseEntity = _mapper.Map<Expense>(expenseDto);

            await _expenseRepository.Create(expenseEntity);
        }

        public async Task UpdateExpenseAsync(ExpenseDTO expenseDto)
        {
            var categoryEntity = _mapper.Map<Expense>(expenseDto);
            await _expenseRepository.Update(categoryEntity);
        }

        public async Task<ExpenseDTO?> DeleteExpenseAsync(int id)
        {
            var expense = await _expenseRepository.Delete(id);

            if (expense == null)
                return null;

            return _mapper.Map<ExpenseDTO>(expense);
        }

    }
}
