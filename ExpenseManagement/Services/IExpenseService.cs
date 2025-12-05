using ExpenseManagement.DTOs;

namespace ExpenseManagement.Services
{
    public interface IExpenseService
    {
        Task<IEnumerable<ExpenseDTO>> GetAllProductsAsync();
        Task GetProductByIdAsync(int id);
        Task CreateProductAsync(ExpenseDTO productDto);
        Task UpdateProductAsync(int id, ExpenseDTO productDto);
        Task DeleteProductAsync(int id);

    }
}
