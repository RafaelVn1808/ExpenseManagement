using ExpenseManagement.Models;

namespace ExpenseManagement.Repositories
{
    public interface ICategoryRepository
    {

        IEnumerable<Category> GetCategories();
        Category GetCategoryById(int categoryId);
        Category Create(Category category);
        Category Update(Category category);
        Category Delete(int categoryId);


    }
}
