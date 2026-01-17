using AutoMapper;
using ExpenseApi.ErrorResponse;
using ExpenseManagement.DTOs;
using ExpenseManagement.DTOs.Mappings;
using ExpenseManagement.Models;
using ExpenseManagement.Repositories;
using ExpenseManagement.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace ExpenseManagement.Tests.Services
{
    public class CategoryServiceTests
    {
        private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
        private readonly IMapper _mapper;
        private readonly CategoryService _categoryService;

        public CategoryServiceTests()
        {
            _categoryRepositoryMock = new Mock<ICategoryRepository>();

            // Configurar AutoMapper
            var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
            _mapper = config.CreateMapper();

            _categoryService = new CategoryService(_categoryRepositoryMock.Object, _mapper);
        }

        [Fact]
        public async Task GetCategories_DeveRetornarListaDeCategorias()
        {
            // Arrange
            var categories = new List<Category>
            {
                new Category { CategoryId = 1, Name = "Alimentação" },
                new Category { CategoryId = 2, Name = "Transporte" }
            };

            _categoryRepositoryMock
                .Setup(r => r.GetCategories())
                .ReturnsAsync(categories);

            // Act
            var result = await _categoryService.GetCategories();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.First().Name.Should().Be("Alimentação");
        }

        [Fact]
        public async Task GetCategoryById_QuandoCategoriaExiste_DeveRetornarCategoria()
        {
            // Arrange
            var categoryId = 1;
            var category = new Category { CategoryId = categoryId, Name = "Alimentação" };

            _categoryRepositoryMock
                .Setup(r => r.GetCategoryById(categoryId))
                .ReturnsAsync(category);

            // Act
            var result = await _categoryService.GetCategoryById(categoryId);

            // Assert
            result.Should().NotBeNull();
            result.CategoryId.Should().Be(categoryId);
            result.Name.Should().Be("Alimentação");
        }

        [Fact]
        public async Task GetCategoryById_QuandoCategoriaNaoExiste_DeveLancarKeyNotFoundException()
        {
            // Arrange
            var categoryId = 999;
            _categoryRepositoryMock
                .Setup(r => r.GetCategoryById(categoryId))
                .ReturnsAsync((Category?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _categoryService.GetCategoryById(categoryId)
            );
        }

        [Fact]
        public async Task AddCategory_ComNomeValido_DeveCriarCategoria()
        {
            // Arrange
            var categoryDto = new CategoryDTO { Name = "Alimentação" };
            var categoryEntity = new Category { CategoryId = 1, Name = "Alimentação" };

            _categoryRepositoryMock
                .Setup(r => r.Create(It.IsAny<Category>()))
                .ReturnsAsync(categoryEntity);

            // Act
            await _categoryService.AddCategory(categoryDto);

            // Assert
            _categoryRepositoryMock.Verify(r => r.Create(It.IsAny<Category>()), Times.Once);
            categoryDto.CategoryId.Should().Be(1);
        }

        [Fact]
        public async Task AddCategory_ComNomeVazio_DeveLancarBusinessException()
        {
            // Arrange
            var categoryDto = new CategoryDTO { Name = "" };

            // Act & Assert
            await Assert.ThrowsAsync<BusinessException>(
                () => _categoryService.AddCategory(categoryDto)
            );

            _categoryRepositoryMock.Verify(r => r.Create(It.IsAny<Category>()), Times.Never);
        }

        [Fact]
        public async Task AddCategory_ComNomeNull_DeveLancarBusinessException()
        {
            // Arrange
            var categoryDto = new CategoryDTO { Name = null! };

            // Act & Assert
            await Assert.ThrowsAsync<BusinessException>(
                () => _categoryService.AddCategory(categoryDto)
            );

            _categoryRepositoryMock.Verify(r => r.Create(It.IsAny<Category>()), Times.Never);
        }

        [Fact]
        public async Task UpdateCategory_QuandoCategoriaExiste_DeveAtualizarCategoria()
        {
            // Arrange
            var categoryDto = new CategoryDTO { CategoryId = 1, Name = "Alimentação Atualizada" };
            var existingCategory = new Category { CategoryId = 1, Name = "Alimentação" };

            _categoryRepositoryMock
                .Setup(r => r.GetCategoryById(1))
                .ReturnsAsync(existingCategory);

            _categoryRepositoryMock
                .Setup(r => r.Update(It.IsAny<Category>()))
                .ReturnsAsync(existingCategory);

            // Act
            await _categoryService.UpdateCategory(categoryDto);

            // Assert
            _categoryRepositoryMock.Verify(r => r.GetCategoryById(1), Times.Once);
            _categoryRepositoryMock.Verify(r => r.Update(It.IsAny<Category>()), Times.Once);
        }

        [Fact]
        public async Task UpdateCategory_QuandoCategoriaNaoExiste_DeveLancarKeyNotFoundException()
        {
            // Arrange
            var categoryDto = new CategoryDTO { CategoryId = 999, Name = "Inexistente" };

            _categoryRepositoryMock
                .Setup(r => r.GetCategoryById(999))
                .ReturnsAsync((Category?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _categoryService.UpdateCategory(categoryDto)
            );

            _categoryRepositoryMock.Verify(r => r.Update(It.IsAny<Category>()), Times.Never);
        }

        [Fact]
        public async Task RemoveCategory_QuandoCategoriaExiste_DeveRemoverCategoria()
        {
            // Arrange
            var categoryId = 1;
            var category = new Category { CategoryId = categoryId, Name = "Alimentação" };

            _categoryRepositoryMock
                .Setup(r => r.GetCategoryById(categoryId))
                .ReturnsAsync(category);

            // Act
            await _categoryService.RemoveCategory(categoryId);

            // Assert
            _categoryRepositoryMock.Verify(r => r.GetCategoryById(categoryId), Times.Once);
            _categoryRepositoryMock.Verify(r => r.Delete(categoryId), Times.Once);
        }

        [Fact]
        public async Task RemoveCategory_QuandoCategoriaNaoExiste_NaoDeveLancarExcecao()
        {
            // Arrange
            var categoryId = 999;
            _categoryRepositoryMock
                .Setup(r => r.GetCategoryById(categoryId))
                .ReturnsAsync((Category?)null);

            // Act
            await _categoryService.RemoveCategory(categoryId);

            // Assert
            _categoryRepositoryMock.Verify(r => r.GetCategoryById(categoryId), Times.Once);
            _categoryRepositoryMock.Verify(r => r.Delete(It.IsAny<int>()), Times.Never);
        }
    }
}
