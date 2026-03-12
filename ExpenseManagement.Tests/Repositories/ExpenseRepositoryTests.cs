using ExpenseManagement.Context;
using ExpenseManagement.DTOs;
using ExpenseManagement.Models;
using ExpenseManagement.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ExpenseManagement.Tests.Repositories
{
    public class ExpenseRepositoryTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly ExpenseRepository _repository;
        private const string TestUserId = "user-123";

        public ExpenseRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            SeedData();
            _repository = new ExpenseRepository(_context);
        }

        private void SeedData()
        {
            _context.Categories!.AddRange(
                new Category { CategoryId = 1, Name = "Alimentação" },
                new Category { CategoryId = 2, Name = "Transporte" });
            _context.SaveChanges();

            _context.Expenses!.AddRange(
                new Expense
                {
                    ExpenseId = 1,
                    Name = "Supermercado",
                    TotalAmount = 100m,
                    UserId = TestUserId,
                    CategoryId = 1,
                    StartDate = DateTime.UtcNow.AddDays(-5),
                    Status = ExpenseStatus.Pendente,
                    Installments = 1,
                    InstallmentAmount = 100m,
                    CreatedAt = DateTime.UtcNow
                },
                new Expense
                {
                    ExpenseId = 2,
                    Name = "Uber",
                    TotalAmount = 50m,
                    UserId = TestUserId,
                    CategoryId = 2,
                    StartDate = DateTime.UtcNow.AddDays(-2),
                    Status = ExpenseStatus.Pago,
                    Installments = 1,
                    InstallmentAmount = 50m,
                    CreatedAt = DateTime.UtcNow
                },
                new Expense
                {
                    ExpenseId = 3,
                    Name = "Restaurante",
                    TotalAmount = 80m,
                    UserId = "other-user",
                    CategoryId = 1,
                    StartDate = DateTime.UtcNow,
                    Status = ExpenseStatus.Pendente,
                    Installments = 1,
                    InstallmentAmount = 80m,
                    CreatedAt = DateTime.UtcNow
                });
            _context.SaveChanges();
        }

        public void Dispose() => _context.Dispose();

        [Fact]
        public async Task GetExpenses_DeveRetornarApenasDespesasDoUsuario()
        {
            var result = await _repository.GetExpenses(TestUserId);

            result.Should().HaveCount(2);
            result.Should().OnlyContain(e => e.UserId == TestUserId);
        }

        [Fact]
        public async Task GetExpensesPaged_SemFiltro_DeveRetornarPaginado()
        {
            var parameters = new ExpenseQueryParameters { Page = 1, PageSize = 10 };
            var result = await _repository.GetExpensesPaged(parameters, TestUserId);

            result.TotalCount.Should().Be(2);
            result.Items.Should().HaveCount(2);
            result.Page.Should().Be(1);
        }

        [Fact]
        public async Task GetExpensesPaged_ComFiltroCategoria_DeveFiltrar()
        {
            var parameters = new ExpenseQueryParameters { Page = 1, PageSize = 10, CategoryId = 1 };
            var result = await _repository.GetExpensesPaged(parameters, TestUserId);

            result.Items.Should().OnlyContain(e => e.CategoryId == 1);
            result.TotalCount.Should().Be(1);
        }

        [Fact]
        public async Task GetExpenseId_QuandoExiste_DeveRetornarDespesa()
        {
            var result = await _repository.GetExpenseId(1, TestUserId);

            result.Should().NotBeNull();
            result!.ExpenseId.Should().Be(1);
            result.Category.Should().NotBeNull();
        }

        [Fact]
        public async Task GetExpenseId_QuandoDeOutroUsuario_DeveRetornarNull()
        {
            var result = await _repository.GetExpenseId(3, TestUserId);

            result.Should().BeNull();
        }

        [Fact]
        public async Task Create_DeveInserirDespesa()
        {
            var expense = new Expense
            {
                Name = "Nova",
                TotalAmount = 200m,
                UserId = TestUserId,
                CategoryId = 1,
                StartDate = DateTime.UtcNow,
                Status = ExpenseStatus.Pendente,
                Installments = 2,
                InstallmentAmount = 100m,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _repository.Create(expense);

            result.ExpenseId.Should().BeGreaterThan(0);
            var loaded = await _context.Expenses!.FindAsync(result.ExpenseId);
            loaded.Should().NotBeNull();
        }

        [Fact]
        public async Task Delete_QuandoExiste_DeveRemover()
        {
            var result = await _repository.Delete(1, TestUserId);

            result.Should().NotBeNull();
            var loaded = await _context.Expenses!.FindAsync(1);
            loaded.Should().BeNull();
        }

        [Fact]
        public async Task Delete_QuandoDeOutroUsuario_DeveRetornarNull()
        {
            var result = await _repository.Delete(3, TestUserId);

            result.Should().BeNull();
        }
    }
}
