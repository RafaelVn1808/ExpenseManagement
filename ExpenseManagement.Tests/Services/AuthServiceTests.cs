using ExpenseApi.Identity;
using ExpenseManagement.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Security.Claims;
using Xunit;

namespace ExpenseManagement.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            // Configurar UserManager Mock
            var store = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                store.Object, null!, null!, null!, null!, null!, null!, null!, null!
            );

            // Configurar IConfiguration
            _configurationMock = new Mock<IConfiguration>();
            _configurationMock.Setup(c => c["Jwt:Key"]).Returns("EstaEhUmaChaveSecretaMuitoLongaParaTesteDoJwtToken123456789");
            _configurationMock.Setup(c => c["Jwt:Issuer"]).Returns("ExpenseManagement");
            _configurationMock.Setup(c => c["Jwt:Audience"]).Returns("ExpenseManagementUsers");

            _authService = new AuthService(_userManagerMock.Object, _configurationMock.Object);
        }

        [Fact]
        public async Task LoginAsync_ComCredenciaisValidas_DeveRetornarToken()
        {
            // Arrange
            var email = "teste@example.com";
            var password = "Senha123!";
            var user = new ApplicationUser
            {
                Id = "user-123",
                Email = email,
                UserName = "teste"
            };

            _userManagerMock
                .Setup(x => x.FindByEmailAsync(email))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(x => x.CheckPasswordAsync(user, password))
                .ReturnsAsync(true);

            _userManagerMock
                .Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "User" });

            // Act
            var token = await _authService.LoginAsync(email, password);

            // Assert
            token.Should().NotBeNullOrEmpty();
            token.Should().Contain(".");
        }

        [Fact]
        public async Task LoginAsync_ComUsuarioInexistente_DeveLancarUnauthorizedAccessException()
        {
            // Arrange
            var email = "inexistente@example.com";
            var password = "Senha123!";

            _userManagerMock
                .Setup(x => x.FindByEmailAsync(email))
                .ReturnsAsync((ApplicationUser?)null);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _authService.LoginAsync(email, password)
            );
        }

        [Fact]
        public async Task LoginAsync_ComSenhaInvalida_DeveLancarUnauthorizedAccessException()
        {
            // Arrange
            var email = "teste@example.com";
            var password = "SenhaErrada";
            var user = new ApplicationUser
            {
                Id = "user-123",
                Email = email,
                UserName = "teste"
            };

            _userManagerMock
                .Setup(x => x.FindByEmailAsync(email))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(x => x.CheckPasswordAsync(user, password))
                .ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _authService.LoginAsync(email, password)
            );
        }

        [Fact]
        public async Task LoginAsync_DeveIncluirClaimsNoToken()
        {
            // Arrange
            var email = "teste@example.com";
            var password = "Senha123!";
            var user = new ApplicationUser
            {
                Id = "user-123",
                Email = email,
                UserName = "teste"
            };

            _userManagerMock
                .Setup(x => x.FindByEmailAsync(email))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(x => x.CheckPasswordAsync(user, password))
                .ReturnsAsync(true);

            _userManagerMock
                .Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "Admin", "User" });

            // Act
            var token = await _authService.LoginAsync(email, password);

            // Assert
            token.Should().NotBeNullOrEmpty();
            
            // Decodificar o token para verificar os claims
            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jsonToken = handler.ReadJwtToken(token);
            
            jsonToken.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == "user-123");
            jsonToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == email);
            jsonToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == "teste");
        }
    }
}
