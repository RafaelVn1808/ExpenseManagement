using Microsoft.AspNetCore.Http;

namespace ExpenseManagement.Services
{
    public class ImageUploadService : IImageUploadService
    {
        private readonly HttpClient _httpClient;
        private readonly string _supabaseUrl;
        private readonly string _supabaseAnonKey;
        private readonly string _storageBucket;

        public ImageUploadService(
            HttpClient httpClient,
            string supabaseUrl,
            string supabaseAnonKey,
            string storageBucket = "expense-images")
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _supabaseUrl = (supabaseUrl ?? "").TrimEnd('/');
            _supabaseAnonKey = supabaseAnonKey ?? "";
            _storageBucket = string.IsNullOrWhiteSpace(storageBucket) ? "expense-images" : storageBucket;
        }

        public async Task<string> SaveExpenseImageAsync(IFormFile file)
        {
            if (string.IsNullOrWhiteSpace(_supabaseUrl) || string.IsNullOrWhiteSpace(_supabaseAnonKey))
            {
                throw new InvalidOperationException(
                    "Supabase não configurado. Defina Supabase__Url e Supabase__AnonKey.");
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(extension))
                extension = ".jpg";
            var fileName = $"{Guid.NewGuid():N}{extension}";
            var objectPath = $"expenses/{fileName}";

            var uploadUrl = $"{_supabaseUrl}/storage/v1/object/{_storageBucket}/{objectPath}";

            using var request = new HttpRequestMessage(HttpMethod.Post, uploadUrl);
            request.Headers.Add("Authorization", $"Bearer {_supabaseAnonKey}");
            request.Headers.Add("Cache-Control", "3600");
            request.Headers.Add("x-upsert", "false");

            using var stream = file.OpenReadStream();
            var bytes = new byte[file.Length];
            await stream.ReadAsync(bytes.AsMemory(0, (int)file.Length));
            request.Content = new ByteArrayContent(bytes);
            request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException(
                    $"Falha no upload para Supabase Storage: {response.StatusCode}. {error}");
            }

            var publicUrl = $"{_supabaseUrl}/storage/v1/object/public/{_storageBucket}/{objectPath}";
            return publicUrl;
        }

        public async Task DeleteExpenseImageAsync(string imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl) ||
                string.IsNullOrWhiteSpace(_supabaseUrl) ||
                string.IsNullOrWhiteSpace(_supabaseAnonKey))
            {
                return;
            }

            var objectPath = ExtractObjectPathFromUrl(imageUrl);
            if (string.IsNullOrEmpty(objectPath))
                return;

            if (!objectPath.StartsWith("expenses/", StringComparison.OrdinalIgnoreCase))
                return;

            var deleteUrl = $"{_supabaseUrl}/storage/v1/object/{_storageBucket}/{objectPath}";

            using var request = new HttpRequestMessage(HttpMethod.Delete, deleteUrl);
            request.Headers.Add("Authorization", $"Bearer {_supabaseAnonKey}");

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode && response.StatusCode != System.Net.HttpStatusCode.NotFound)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException(
                    $"Falha ao remover imagem do Supabase Storage: {response.StatusCode}. {error}");
            }
        }

        private string? ExtractObjectPathFromUrl(string imageUrl)
        {
            const string publicPrefix = "/storage/v1/object/public/";
            var uri = imageUrl.Trim();

            if (Uri.TryCreate(uri, UriKind.Absolute, out var parsedUri))
            {
                var path = parsedUri.AbsolutePath;
                var idx = path.IndexOf(publicPrefix, StringComparison.OrdinalIgnoreCase);
                if (idx >= 0)
                {
                    var afterPrefix = path[(idx + publicPrefix.Length)..];
                    var bucketAndPath = afterPrefix.Split('/', 2);
                    if (bucketAndPath.Length == 2)
                        return bucketAndPath[1];
                    return null;
                }
            }

            if (uri.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
            {
                var parts = uri.TrimStart('/').Split('/');
                if (parts.Length >= 2)
                    return $"expenses/{parts[^1]}";
            }

            return null;
        }
    }
}
