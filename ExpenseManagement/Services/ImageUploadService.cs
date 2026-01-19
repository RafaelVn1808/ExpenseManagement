using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace ExpenseManagement.Services
{
    public class ImageUploadService : IImageUploadService
    {
        private readonly IWebHostEnvironment _environment;

        public ImageUploadService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string> SaveExpenseImageAsync(IFormFile file)
        {
            var webRoot = _environment.WebRootPath
                ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

            var uploadsPath = Path.Combine(webRoot, "uploads", "expenses");
            Directory.CreateDirectory(uploadsPath);

            var extension = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(uploadsPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/expenses/{fileName}";
        }
    }
}
