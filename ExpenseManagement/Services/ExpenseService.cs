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

        public async Task<IEnumerable<ExpenseDTO>> GetAllExpensesAsync()
        {
            var expensesEntity = await _expenseRepository.GetExpenses();
            return _mapper.Map<IEnumerable<ExpenseDTO>>(expensesEntity);
        }

        public async Task<ExpenseDTO> GetExpensesByIdAsync(int id)
        {
            var expenseEntity = await _expenseRepository.GetExpenseId(id);

            if (expenseEntity == null)
                throw new KeyNotFoundException("Despesa não encontrada");

            return _mapper.Map<ExpenseDTO>(expenseEntity);
        }

        public async Task CreateExpensesAsync(ExpenseDTO expenseDto)
        {
            if (expenseDto.TotalAmount <= 0)
                throw new BusinessException("O valor total deve ser maior que zero");

            if (expenseDto.Installments <= 0)
                expenseDto.Installments = 1;

            expenseDto.InstallmentAmount =
                Math.Round(expenseDto.TotalAmount / expenseDto.Installments, 2);

            var expenseEntity = _mapper.Map<Expense>(expenseDto);

            await _expenseRepository.Create(expenseEntity);
        }

        public async Task UpdateExpenseAsync(ExpenseDTO expenseDto)
        {
            if (expenseDto.Installments <= 0)
                expenseDto.Installments = 1;

            expenseDto.InstallmentAmount =
                Math.Round(expenseDto.TotalAmount / expenseDto.Installments, 2);

            var expenseEntity = _mapper.Map<Expense>(expenseDto);
            await _expenseRepository.Update(expenseEntity);
        }

        public async Task<ExpenseDTO> DeleteExpenseAsync(int id)
        {
            var expense = await _expenseRepository.Delete(id);

            if (expense == null)
                throw new KeyNotFoundException("Despesa não encontrada");

            return _mapper.Map<ExpenseDTO>(expense);
        }
    }
}
