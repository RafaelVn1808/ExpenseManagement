using ExpenseManagement.DTOs;
using ExpenseManagement.Repositories;

namespace ExpenseManagement.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public Task<IEnumerable<CategoryDTO>> GetCategories()
        {
            throw new NotImplementedException();
        }

        public Task<CategoryDTO> GetCategoryById(int categoryId)
        {
            throw new NotImplementedException();
        }
        public Task<CategoryDTO> AddCategory(CategoryDTO category)
        {
            throw new NotImplementedException();
        }

        public Task<CategoryDTO> UpdateCategory(CategoryDTO category)
        {
            throw new NotImplementedException();
        }

        public Task<CategoryDTO> RemoveCategory(int categoryId)
        {
            throw new NotImplementedException();
        }

        
    }
}
