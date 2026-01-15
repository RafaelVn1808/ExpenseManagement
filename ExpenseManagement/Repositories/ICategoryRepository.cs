using ExpenseManagement.Models;

namespace ExpenseManagement.Repositories
{
    public interface ICategoryRepository
    {

        Task<IEnumerable<Category>> GetCategories();
        Task<Category?> GetCategoryById(int categoryId);
        Task<Category> Create(Category category);
        Task<Category> Update(Category category);
        Task<Category?> Delete(int categoryId);


    }
}
