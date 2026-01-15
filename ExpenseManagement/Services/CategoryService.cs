using AutoMapper;
using ExpenseManagement.DTOs;
using ExpenseManagement.Models;
using ExpenseManagement.Repositories;

namespace ExpenseManagement.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public CategoryService(ICategoryRepository categoryRepository, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CategoryDTO>> GetCategories()
        {
            var categoriesEntity = await _categoryRepository.GetCategories();
            return _mapper.Map<IEnumerable<CategoryDTO>>(categoriesEntity);
        }

        public async Task<CategoryDTO> GetCategoryById(int categoryId)
        {
            var categoriesEntity = await _categoryRepository.GetCategoryById(categoryId);
            return _mapper.Map<CategoryDTO>(categoriesEntity);
        }
        public async Task AddCategory(CategoryDTO category)
        {
            var categoryEntity = _mapper.Map<Category>(category);
            await _categoryRepository.Create(categoryEntity);
            category.CategoryId = categoryEntity.CategoryId;
        }

        public async Task UpdateCategory(CategoryDTO categoryDto)
        {
            var categoryEntity = _mapper.Map<Category>(categoryDto);
            await _categoryRepository.Update(categoryEntity);
        }

        public async Task RemoveCategory(int categoryId)
        {
            var categoryEntity = await _categoryRepository.GetCategoryById(categoryId);
            if (categoryEntity != null)
            {
                await _categoryRepository.Delete(categoryEntity.CategoryId);
            }
        }

       
    }
}
