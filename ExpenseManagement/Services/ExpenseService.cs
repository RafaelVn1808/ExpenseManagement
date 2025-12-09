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
            var expenseEntity = _mapper.Map<Expense>(expenseDto);
            await _expenseRepository.Create(expenseEntity);
            expenseDto.Category = expenseEntity.Category;
        }

        public async Task UpdateExpenseAsync(ExpenseDTO expenseDto)
        {
            var categoryEntity = _mapper.Map<Expense>(expenseDto);
            await _expenseRepository.Update(categoryEntity);
        }

        public async Task DeleteExpenseAsync(int id)
        {
            var expenseEntity = _expenseRepository.GetExpenseId(id).Result;
            await _expenseRepository.Delete(expenseEntity.ExpenseId);
        }
    }
}
