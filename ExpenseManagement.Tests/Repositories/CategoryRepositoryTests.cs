using ExpenseManagement.Context;
using ExpenseManagement.Models;
using ExpenseManagement.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ExpenseManagement.Tests.Repositories
{
    public class CategoryRepositoryTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly CategoryRepository _repository;

        public CategoryRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _context.Categories!.AddRange(
                new Category { CategoryId = 1, Name = "Alimentação" },
                new Category { CategoryId = 2, Name = "Transporte" });
            _context.SaveChanges();

            _repository = new CategoryRepository(_context);
        }

        public void Dispose() => _context.Dispose();

        [Fact]
        public async Task GetCategories_DeveRetornarTodas()
        {
            var result = await _repository.GetCategories();

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetCategoryById_QuandoExiste_DeveRetornar()
        {
            var result = await _repository.GetCategoryById(1);

            result.Should().NotBeNull();
            result!.Name.Should().Be("Alimentação");
        }

        [Fact]
        public async Task GetCategoryById_QuandoNaoExiste_DeveRetornarNull()
        {
            var result = await _repository.GetCategoryById(999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task Create_DeveInserirCategoria()
        {
            var category = new Category { Name = "Lazer" };

            var result = await _repository.Create(category);

            result.CategoryId.Should().BeGreaterThan(0);
            result.Name.Should().Be("Lazer");
        }

        [Fact]
        public async Task Update_DeveAtualizarCategoria()
        {
            var category = await _repository.GetCategoryById(1);
            category.Should().NotBeNull();
            category!.Name = "Alimentação Atualizada";

            await _repository.Update(category);

            var loaded = await _repository.GetCategoryById(1);
            loaded!.Name.Should().Be("Alimentação Atualizada");
        }

        [Fact]
        public async Task Delete_DeveRemoverCategoria()
        {
            await _repository.Delete(2);

            var loaded = await _context.Categories!.FindAsync(2);
            loaded.Should().BeNull();
        }
    }
}
