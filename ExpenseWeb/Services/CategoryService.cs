using ExpenseWeb.Models;
using ExpenseWeb.Services.Contracts;
using Microsoft.Extensions.Caching.Memory;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace ExpenseWeb.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IMemoryCache _cache;
        private readonly JsonSerializerOptions _options;
        private const string apiEndpoint = "/api/category";
        private const string CacheKey = "categories_all";
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

        public CategoryService(IHttpClientFactory clientFactory, IMemoryCache cache)
        {
            _clientFactory = clientFactory;
            _cache = cache;
            _options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase // Usar camelCase na serialização
            };
        }

        public async Task<IEnumerable<CategoryViewModel>> GetAllCategories()
        {
            return await _cache.GetOrCreateAsync(CacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheDuration;
                var client = _clientFactory.CreateClient("ExpenseApi");
                var response = await client.GetAsync(apiEndpoint);
                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = await response.Content.ReadAsStreamAsync();
                    var categories = await JsonSerializer.DeserializeAsync<IEnumerable<CategoryViewModel>>(apiResponse, _options);
                    return categories ?? Enumerable.Empty<CategoryViewModel>();
                }
                return Enumerable.Empty<CategoryViewModel>();
            });
        }

        public async Task<CategoryViewModel?> CreateCategory(CategoryViewModel category)
        {
            var client = _clientFactory.CreateClient("ExpenseApi");

            // Mapear CategoryViewModel para CategoryDTO (formato esperado pela API)
            // CategoryDTO requer apenas Name (CategoryId será gerado pela API)
            var categoryDto = new
            {
                Name = category.Name // O JsonNamingPolicy.CamelCase converterá para "name"
            };

            var jsonContent = JsonSerializer.Serialize(categoryDto);
            System.Diagnostics.Debug.WriteLine($"Enviando para API: {jsonContent}");
            
            var content = new StringContent(
                jsonContent,
                Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync(apiEndpoint, content);
            System.Diagnostics.Debug.WriteLine($"Resposta da API: {response.StatusCode}");
            
            if (response.IsSuccessStatusCode)
            {
                // CreatedAtRouteResult retorna o objeto CategoryDTO no body
                var responseContent = await response.Content.ReadAsStringAsync();
                
                // Tentar deserializar diretamente
                var createdCategory = JsonSerializer.Deserialize<CategoryViewModel>(responseContent, _options);
                
                if (createdCategory != null)
                {
                    _cache.Remove(CacheKey);
                    return createdCategory;
                }
                
                // Se falhar, tentar deserializar como CategoryDTO e mapear
                var categoryDtoResponse = JsonSerializer.Deserialize<CategoryApiDto>(responseContent, _options);
                if (categoryDtoResponse != null)
                {
                    _cache.Remove(CacheKey);
                    return new CategoryViewModel
                    {
                        CategoryId = categoryDtoResponse.CategoryId,
                        Name = categoryDtoResponse.Name ?? string.Empty
                    };
                }
            }
            
            // Log do erro para debug
            var errorContent = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"Erro ao criar categoria: {response.StatusCode} - {errorContent}");
            
            // Lançar exceção com detalhes do erro para melhor tratamento
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException("Não autorizado. Verifique se você está logado e se o token é válido.");
            }
            
            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                throw new UnauthorizedAccessException("Acesso negado. Verifique se está logado e tente novamente.");
            }
            
            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                // Tentar extrair mensagens de validação
                try
                {
                    var errorObj = JsonSerializer.Deserialize<Dictionary<string, object>>(errorContent, _options);
                    var errorMessage = errorObj?.ContainsKey("title") == true 
                        ? errorObj["title"]?.ToString() 
                        : errorContent;
                    throw new InvalidOperationException($"Erro de validação: {errorMessage}");
                }
                catch
                {
                    throw new InvalidOperationException($"Erro ao criar categoria: {errorContent}");
                }
            }
            
            throw new InvalidOperationException($"Erro ao criar categoria (Status: {response.StatusCode}): {errorContent}");
        }

        public async Task<bool> DeleteCategory(int id)
        {
            var client = _clientFactory.CreateClient("ExpenseApi");
            var response = await client.DeleteAsync($"{apiEndpoint}/{id}");
            if (response.IsSuccessStatusCode)
                _cache.Remove(CacheKey);
            return response.IsSuccessStatusCode;
        }
        
        // DTO auxiliar para deserialização da API
        private class CategoryApiDto
        {
            public int CategoryId { get; set; }
            public string? Name { get; set; }
        }
    }
}
