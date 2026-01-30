using ExpenseWeb.Models;
using ExpenseWeb.Services.Contracts;
using System.Text;
using System.Text.Json;
using System.Linq;

namespace ExpenseWeb.Services
{
    public class AdminService : IAdminService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly JsonSerializerOptions _options;

        public AdminService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public async Task<IReadOnlyList<AdminUserViewModel>> GetUsersAsync()
        {
            var client = _httpClientFactory.CreateClient("ExpenseApi");
            var response = await client.GetAsync("api/admin/users");

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException(
                    "Não foi possível carregar a lista de usuários. Faça login novamente como administrador.");
            }
            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                throw new UnauthorizedAccessException(
                    "Acesso negado. Apenas administradores podem acessar a lista de usuários.");
            }
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException(
                    $"Não foi possível carregar a lista de usuários. (Status: {response.StatusCode}) {body}");
            }

            var json = await response.Content.ReadAsStringAsync();
            var users = JsonSerializer.Deserialize<List<AdminUserViewModel>>(json, _options);
            return users ?? new List<AdminUserViewModel>();
        }

        public async Task<bool> UpdateUserRolesAsync(string userId, IEnumerable<string> roles)
        {
            var client = _httpClientFactory.CreateClient("ExpenseApi");

            var payload = new
            {
                roles = roles.ToList()
            };

            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync($"api/admin/users/{userId}/roles", content);
            return response.IsSuccessStatusCode;
        }
    }
}
