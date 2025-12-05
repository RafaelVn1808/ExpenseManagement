

using ExpenseManagement.DTOs;

namespace ExpenseManagement.Services
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDTO>> GetCategories();
        Task<CategoryDTO> GetCategoryById(int categoryId);
        Task<CategoryDTO> AddCategory(CategoryDTO category);
        Task<CategoryDTO> UpdateCategory(CategoryDTO category);
        Task<CategoryDTO> RemoveCategory(int categoryId);
    }
}
