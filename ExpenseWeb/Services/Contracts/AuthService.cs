using ExpenseWeb.Models;
using ExpenseWeb.Services.Contracts;
using System.Text;
using System.Text.Json;

namespace ExpenseWeb.Services
{
    public class AuthService : IAuthService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AuthService(
            IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<LoginResponse?> LoginAsync(string email, string password)
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
            return JsonSerializer.Deserialize<LoginResponse>(json);
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

        public async Task<string?> ChangePasswordAsync(string currentPassword, string newPassword)
        {
            var client = _httpClientFactory.CreateClient("ExpenseApi");
            var payload = new { currentPassword, newPassword };
            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");
            var response = await client.PostAsync("api/auth/change-password", content);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                try
                {
                    var obj = JsonSerializer.Deserialize<JsonElement>(body);
                    if (obj.TryGetProperty("message", out var msg))
                        return msg.GetString();
                    if (obj.TryGetProperty("errors", out var errs))
                        return errs.ToString();
                }
                catch { }
                return "Não foi possível alterar a senha.";
            }
            return null;
        }

        public async Task<bool> ForgotPasswordAsync(string email)
        {
            var client = _httpClientFactory.CreateClient("ExpenseApi");
            var payload = new { email };
            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");
            var response = await client.PostAsync("api/auth/forgot-password", content);
            return response.IsSuccessStatusCode;
        }
    }
}
