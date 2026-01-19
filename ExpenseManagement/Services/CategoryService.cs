using AutoMapper;
using ExpenseApi.ErrorResponse;
using ExpenseManagement.DTOs;
using ExpenseManagement.Models;
using ExpenseManagement.Repositories;
using Microsoft.Extensions.Caching.Memory;

namespace ExpenseManagement.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;
        private const string CategoriesCacheKey = "categories:all";

        public CategoryService(ICategoryRepository categoryRepository, IMapper mapper, IMemoryCache cache)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
            _cache = cache;
        }

        public async Task<IEnumerable<CategoryDTO>> GetCategories()
        {
            if (_cache.TryGetValue(CategoriesCacheKey, out IEnumerable<CategoryDTO>? cached) && cached != null)
            {
                return cached;
            }

            var categoriesEntity = await _categoryRepository.GetCategories();
            var categoriesDto = _mapper.Map<IEnumerable<CategoryDTO>>(categoriesEntity);

            _cache.Set(CategoriesCacheKey, categoriesDto, TimeSpan.FromMinutes(10));

            return categoriesDto;
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
            var createdCategory = await _categoryRepository.Create(categoryEntity);

            category.CategoryId = createdCategory.CategoryId;
            _cache.Remove(CategoriesCacheKey);
        }

        public async Task UpdateCategory(CategoryDTO categoryDto)
        {
            var existing = await _categoryRepository.GetCategoryById(categoryDto.CategoryId);

            if (existing == null)
                throw new KeyNotFoundException("Categoria não encontrada");

            var categoryEntity = _mapper.Map<Category>(categoryDto);
            await _categoryRepository.Update(categoryEntity);
            _cache.Remove(CategoriesCacheKey);
        }

        public async Task RemoveCategory(int categoryId)
        {
            var categoryEntity = await _categoryRepository.GetCategoryById(categoryId);
            if (categoryEntity != null)
            {
                await _categoryRepository.Delete(categoryEntity.CategoryId);
                _cache.Remove(CategoriesCacheKey);
            }
        }
    }
}
