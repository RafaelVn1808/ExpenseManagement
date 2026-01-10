using ExpenseWeb.Models;
using ExpenseWeb.Services.Contracts;
using System.Text.Json;

namespace ExpenseWeb.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly JsonSerializerOptions _options;
        private const string apiEndpoint = "/api/category/";

        public CategoryService(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
            _options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<IEnumerable<CategoryViewModel>> GetAllCategories()
        {
            var client = _clientFactory.CreateClient("ExpenseApi");

            IEnumerable<CategoryViewModel> categories;

            var response = await client.GetAsync(apiEndpoint);
            if (response.IsSuccessStatusCode)
            {
                var apiResponse = await response.Content.ReadAsStreamAsync();
                categories = await JsonSerializer.DeserializeAsync<IEnumerable<CategoryViewModel>>(apiResponse, _options);
            }
            else
            {
                return null;
            }
            return categories;
        }
    }
}
