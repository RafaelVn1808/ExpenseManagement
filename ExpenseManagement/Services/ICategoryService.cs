

using ExpenseManagement.DTOs;

namespace ExpenseManagement.Services
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDTO>> GetCategories();
        Task<CategoryDTO> GetCategoryById(int categoryId);
        Task AddCategory(CategoryDTO category);
        Task UpdateCategory(CategoryDTO category);
        Task RemoveCategory(int categoryId);
    }
}
