using AutoMapper;
using ExpenseApi.ErrorResponse;
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
            var categoryEntity = await _categoryRepository.GetCategoryById(categoryId);

            if (categoryEntity == null)
                throw new KeyNotFoundException("Categoria não encontrada");

            return _mapper.Map<CategoryDTO>(categoryEntity);
        }

        public async Task AddCategory(CategoryDTO category)
        {
            if (string.IsNullOrWhiteSpace(category.Name))
                throw new BusinessException("O nome da categoria é obrigatório");

            var categoryEntity = _mapper.Map<Category>(category);
            await _categoryRepository.Create(categoryEntity);

            category.CategoryId = categoryEntity.CategoryId;
        }

        public async Task UpdateCategory(CategoryDTO categoryDto)
        {
            var existing = await _categoryRepository.GetCategoryById(categoryDto.CategoryId);

            if (existing == null)
                throw new KeyNotFoundException("Categoria não encontrada");

            var categoryEntity = _mapper.Map<Category>(categoryDto);
            await _categoryRepository.Update(categoryEntity);
        }

        public async Task RemoveCategory(int categoryId)
        {
            var categoryEntity = await _categoryRepository.GetCategoryById(categoryId);

            if (categoryEntity == null)
                throw new KeyNotFoundException("Categoria não encontrada");

            await _categoryRepository.Delete(categoryEntity.CategoryId);
        }
    }
}
