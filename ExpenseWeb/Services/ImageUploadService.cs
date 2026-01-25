using System.Net.Http.Headers;
using System.Text.Json;

namespace ExpenseWeb.Services
{
    public class ImageUploadService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private const long MaxFileSize = 5 * 1024 * 1024; // 5MB
        private readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        public ImageUploadService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<string?> UploadImageAsync(IFormFile? file, string prefix = "img")
        {
            if (file == null || file.Length == 0)
                return null;

            _ = prefix;

            // Validar tamanho
            if (file.Length > MaxFileSize)
                throw new InvalidOperationException($"O arquivo excede o tamanho máximo de {MaxFileSize / 1024 / 1024}MB.");

            // Validar extensão
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
                throw new InvalidOperationException($"Extensão de arquivo não permitida. Use: {string.Join(", ", AllowedExtensions)}");

            using var content = new MultipartFormDataContent();
            using var stream = file.OpenReadStream();
            var streamContent = new StreamContent(stream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            content.Add(streamContent, "file", file.FileName);

            var client = _httpClientFactory.CreateClient("ExpenseApi");
            var response = await client.PostAsync("/api/expense/upload", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Falha no upload: {response.StatusCode} - {error}");
            }

            var json = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(json);
            return document.RootElement.GetProperty("url").GetString();
        }

        public async Task DeleteImageAsync(string? imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                return;
            }

            var client = _httpClientFactory.CreateClient("ExpenseApi");
            var payload = new { url = imageUrl };
            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                System.Text.Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync("/api/expense/delete-image", content);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Falha ao excluir imagem: {response.StatusCode} - {error}");
            }
        }
    }
}
