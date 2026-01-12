using ExpenseWeb.Models;
using ExpenseWeb.Services.Contracts;

namespace ExpenseWeb.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _client;

        public AuthService(IHttpClientFactory factory)
        {
            _client = factory.CreateClient("ExpenseApi");
        }

        public async Task<string?> Login(LoginViewModel model)
        {
            var response = await _client.PostAsJsonAsync("api/auth/login", model);

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadFromJsonAsync<LoginResponse>();
            return json?.Token;
        }
    }
}
