using ExpenseWeb.Models;

namespace ExpenseWeb.Services.Contracts
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryViewModel>> GetAllCategories();
    }
}
