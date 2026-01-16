using ExpenseWeb.Services.Contracts;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ExpenseWeb.Services
{
    public class AuthService : IAuthService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public AuthService(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<string?> LoginAsync(string email, string password)
        {
            var client = _httpClientFactory.CreateClient("ExpenseApi");

            var payload = new
            {
                email,
                password
            };

            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync("api/auth/login", content);

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();

            using var document = JsonDocument.Parse(json);

            return document
                .RootElement
                .GetProperty("token")
                .GetString();
        }

        public async Task<bool> RegisterAsync(string email, string password)
        {
            var client = _httpClientFactory.CreateClient("ExpenseApi");

            var payload = new
            {
                email,
                password,
                confirmPassword = password
            };

            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync("api/auth/register", content);

            return response.IsSuccessStatusCode;
        }
    }
}
