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
    public class ExpenseServiceTests
    {
        private readonly Mock<IExpenseRepository> _expenseRepositoryMock;
        private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
        private readonly IMapper _mapper;
        private readonly ExpenseService _expenseService;
        private readonly string _testUserId = "user-123";

        public ExpenseServiceTests()
        {
            _expenseRepositoryMock = new Mock<IExpenseRepository>();
            _categoryRepositoryMock = new Mock<ICategoryRepository>();

            // Configurar AutoMapper
            var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
            _mapper = config.CreateMapper();

            _categoryRepositoryMock
                .Setup(r => r.GetCategoryById(It.IsAny<int>()))
                .ReturnsAsync(new Category { CategoryId = 1, Name = "Teste" });

            _expenseService = new ExpenseService(
                _expenseRepositoryMock.Object,
                _categoryRepositoryMock.Object,
                _mapper);
        }

        [Fact]
        public async Task GetAllExpensesAsync_DeveRetornarListaDeDespesas()
        {
            // Arrange
            var expenses = new List<Expense>
            {
                new Expense 
                { 
                    ExpenseId = 1, 
                    Name = "Supermercado", 
                    TotalAmount = 100.00m,
                    UserId = _testUserId
                },
                new Expense 
                { 
                    ExpenseId = 2, 
                    Name = "Transporte", 
                    TotalAmount = 50.00m,
                    UserId = _testUserId
                }
            };

            _expenseRepositoryMock
                .Setup(r => r.GetExpenses(_testUserId))
                .ReturnsAsync(expenses);

            // Act
            var result = await _expenseService.GetAllExpensesAsync(_testUserId);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.First().Name.Should().Be("Supermercado");
        }

        [Fact]
        public async Task GetExpensesPagedAsync_DeveRetornarPaginado()
        {
            // Arrange
            var parameters = new ExpenseQueryParameters
            {
                Page = 1,
                PageSize = 2
            };

            var expenses = new List<Expense>
            {
                new Expense { ExpenseId = 1, Name = "Supermercado", TotalAmount = 100.00m, UserId = _testUserId },
                new Expense { ExpenseId = 2, Name = "Transporte", TotalAmount = 50.00m, UserId = _testUserId }
            };

            var paged = new PagedResult<Expense>
            {
                Page = 1,
                PageSize = 2,
                TotalCount = 2,
                Items = expenses
            };

            _expenseRepositoryMock
                .Setup(r => r.GetExpensesPaged(parameters, _testUserId))
                .ReturnsAsync(paged);

            // Act
            var result = await _expenseService.GetExpensesPagedAsync(parameters, _testUserId);

            // Assert
            result.Items.Should().HaveCount(2);
            result.TotalCount.Should().Be(2);
            result.Page.Should().Be(1);
        }

        [Fact]
        public async Task GetExpensesByIdAsync_QuandoDespesaExiste_DeveRetornarDespesa()
        {
            // Arrange
            var expenseId = 1;
            var expense = new Expense 
            { 
                ExpenseId = expenseId, 
                Name = "Supermercado", 
                TotalAmount = 100.00m,
                UserId = _testUserId
            };

            _expenseRepositoryMock
                .Setup(r => r.GetExpenseId(expenseId, _testUserId))
                .ReturnsAsync(expense);

            // Act
            var result = await _expenseService.GetExpensesByIdAsync(expenseId, _testUserId);

            // Assert
            result.Should().NotBeNull();
            result!.ExpenseId.Should().Be(expenseId);
            result.Name.Should().Be("Supermercado");
        }

        [Fact]
        public async Task GetExpensesByIdAsync_QuandoDespesaNaoExiste_DeveRetornarNull()
        {
            // Arrange
            var expenseId = 999;
            _expenseRepositoryMock
                .Setup(r => r.GetExpenseId(expenseId, _testUserId))
                .ReturnsAsync((Expense?)null);

            // Act
            var result = await _expenseService.GetExpensesByIdAsync(expenseId, _testUserId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task CreateExpensesAsync_ComDadosValidos_DeveCriarDespesa()
        {
            // Arrange
            var expenseDto = new ExpenseDTO
            {
                Name = "Supermercado",
                TotalAmount = 100.00m,
                Installments = 1,
                StartDate = DateTime.Today,
                Status = ExpenseStatus.Pendente,
                CategoryId = 1
            };

            var expenseEntity = new Expense
            {
                ExpenseId = 1,
                Name = expenseDto.Name,
                TotalAmount = expenseDto.TotalAmount,
                Installments = expenseDto.Installments,
                StartDate = expenseDto.StartDate,
                Status = expenseDto.Status,
                CategoryId = expenseDto.CategoryId,
                UserId = _testUserId
            };

            _expenseRepositoryMock
                .Setup(r => r.Create(It.IsAny<Expense>()))
                .ReturnsAsync(expenseEntity);

            // Act
            await _expenseService.CreateExpensesAsync(expenseDto, _testUserId);

            // Assert
            _expenseRepositoryMock.Verify(
                r => r.Create(It.Is<Expense>(e => e.UserId == _testUserId)), 
                Times.Once
            );
        }

        [Fact]
        public async Task CreateExpensesAsync_ComValorZero_DeveLancarBusinessException()
        {
            // Arrange
            var expenseDto = new ExpenseDTO
            {
                Name = "Supermercado",
                TotalAmount = 0m,
                Installments = 1,
                StartDate = DateTime.Today,
                Status = ExpenseStatus.Pendente,
                CategoryId = 1
            };

            // Act & Assert
            await Assert.ThrowsAsync<BusinessException>(
                () => _expenseService.CreateExpensesAsync(expenseDto, _testUserId)
            );

            _expenseRepositoryMock.Verify(r => r.Create(It.IsAny<Expense>()), Times.Never);
        }

        [Fact]
        public async Task CreateExpensesAsync_ComValorNegativo_DeveLancarBusinessException()
        {
            // Arrange
            var expenseDto = new ExpenseDTO
            {
                Name = "Supermercado",
                TotalAmount = -10m,
                Installments = 1,
                StartDate = DateTime.Today,
                Status = ExpenseStatus.Pendente,
                CategoryId = 1
            };

            // Act & Assert
            await Assert.ThrowsAsync<BusinessException>(
                () => _expenseService.CreateExpensesAsync(expenseDto, _testUserId)
            );

            _expenseRepositoryMock.Verify(r => r.Create(It.IsAny<Expense>()), Times.Never);
        }

        [Fact]
        public async Task CreateExpensesAsync_ComDataFutura_DeveLancarBusinessException()
        {
            // Arrange
            var expenseDto = new ExpenseDTO
            {
                Name = "Supermercado",
                TotalAmount = 100.00m,
                Installments = 1,
                StartDate = DateTime.Today.AddDays(1),
                Status = ExpenseStatus.Pendente,
                CategoryId = 1
            };

            // Act & Assert
            await Assert.ThrowsAsync<BusinessException>(
                () => _expenseService.CreateExpensesAsync(expenseDto, _testUserId)
            );

            _expenseRepositoryMock.Verify(r => r.Create(It.IsAny<Expense>()), Times.Never);
        }

        [Fact]
        public async Task CreateExpensesAsync_ComParcelasZero_DeveDefinirParcelaComoUm()
        {
            // Arrange
            var expenseDto = new ExpenseDTO
            {
                Name = "Supermercado",
                TotalAmount = 100.00m,
                Installments = 0,
                StartDate = DateTime.Today,
                Status = ExpenseStatus.Pendente,
                CategoryId = 1
            };

            var expenseEntity = new Expense
            {
                ExpenseId = 1,
                UserId = _testUserId
            };

            _expenseRepositoryMock
                .Setup(r => r.Create(It.IsAny<Expense>()))
                .ReturnsAsync(expenseEntity);

            // Act
            await _expenseService.CreateExpensesAsync(expenseDto, _testUserId);

            // Assert
            expenseDto.Installments.Should().Be(1);
            expenseDto.InstallmentAmount.Should().Be(100.00m);
        }

        [Fact]
        public async Task CreateExpensesAsync_ComParcelas_DeveCalcularValorPorParcela()
        {
            // Arrange
            var expenseDto = new ExpenseDTO
            {
                Name = "Supermercado",
                TotalAmount = 100.00m,
                Installments = 3,
                StartDate = DateTime.Today,
                Status = ExpenseStatus.Pendente,
                CategoryId = 1
            };

            var expenseEntity = new Expense
            {
                ExpenseId = 1,
                UserId = _testUserId
            };

            _expenseRepositoryMock
                .Setup(r => r.Create(It.IsAny<Expense>()))
                .ReturnsAsync(expenseEntity);

            // Act
            await _expenseService.CreateExpensesAsync(expenseDto, _testUserId);

            // Assert
            expenseDto.InstallmentAmount.Should().Be(33.33m);
        }

        [Fact]
        public async Task UpdateExpenseAsync_QuandoDespesaExiste_DeveAtualizarDespesa()
        {
            // Arrange
            var expenseDto = new ExpenseDTO
            {
                ExpenseId = 1,
                Name = "Supermercado Atualizado",
                TotalAmount = 150.00m,
                Installments = 2,
                StartDate = DateTime.Today,
                Status = ExpenseStatus.Pago,
                CategoryId = 1
            };

            var existingExpense = new Expense
            {
                ExpenseId = 1,
                Name = "Supermercado",
                TotalAmount = 100.00m,
                UserId = _testUserId
            };

            _expenseRepositoryMock
                .Setup(r => r.GetExpenseId(1, _testUserId))
                .ReturnsAsync(existingExpense);

            _expenseRepositoryMock
                .Setup(r => r.Update(It.IsAny<Expense>()))
                .ReturnsAsync(existingExpense);

            // Act
            await _expenseService.UpdateExpenseAsync(expenseDto, _testUserId);

            // Assert
            _expenseRepositoryMock.Verify(r => r.GetExpenseId(1, _testUserId), Times.Once);
            _expenseRepositoryMock.Verify(
                r => r.Update(It.Is<Expense>(e => e.UserId == _testUserId)), 
                Times.Once
            );
            expenseDto.InstallmentAmount.Should().Be(75.00m);
        }

        [Fact]
        public async Task UpdateExpenseAsync_QuandoDespesaNaoExiste_DeveLancarUnauthorizedAccessException()
        {
            // Arrange
            var expenseDto = new ExpenseDTO
            {
                ExpenseId = 999,
                Name = "Inexistente",
                TotalAmount = 100.00m,
                StartDate = DateTime.Today,
                Status = ExpenseStatus.Pendente,
                CategoryId = 1
            };

            _expenseRepositoryMock
                .Setup(r => r.GetExpenseId(999, _testUserId))
                .ReturnsAsync((Expense?)null);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _expenseService.UpdateExpenseAsync(expenseDto, _testUserId)
            );

            _expenseRepositoryMock.Verify(r => r.Update(It.IsAny<Expense>()), Times.Never);
        }

        [Fact]
        public async Task DeleteExpenseAsync_QuandoDespesaExiste_DeveRetornarDespesaDeletada()
        {
            // Arrange
            var expenseId = 1;
            var expense = new Expense
            {
                ExpenseId = expenseId,
                Name = "Supermercado",
                TotalAmount = 100.00m,
                UserId = _testUserId
            };

            _expenseRepositoryMock
                .Setup(r => r.Delete(expenseId, _testUserId))
                .ReturnsAsync(expense);

            // Act
            var result = await _expenseService.DeleteExpenseAsync(expenseId, _testUserId);

            // Assert
            result.Should().NotBeNull();
            result!.ExpenseId.Should().Be(expenseId);
            _expenseRepositoryMock.Verify(r => r.Delete(expenseId, _testUserId), Times.Once);
        }

        [Fact]
        public async Task DeleteExpenseAsync_QuandoDespesaNaoExiste_DeveRetornarNull()
        {
            // Arrange
            var expenseId = 999;
            _expenseRepositoryMock
                .Setup(r => r.Delete(expenseId, _testUserId))
                .ReturnsAsync((Expense?)null);

            // Act
            var result = await _expenseService.DeleteExpenseAsync(expenseId, _testUserId);

            // Assert
            result.Should().BeNull();
            _expenseRepositoryMock.Verify(r => r.Delete(expenseId, _testUserId), Times.Once);
        }
    }
}
