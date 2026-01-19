using ExpenseWeb.Models;

namespace ExpenseWeb.Services.Contracts
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryViewModel>> GetAllCategories();
        Task<CategoryViewModel?> CreateCategory(CategoryViewModel category);
    }
}
