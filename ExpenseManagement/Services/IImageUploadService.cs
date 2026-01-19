using Microsoft.AspNetCore.Http;

namespace ExpenseManagement.Services
{
    public interface IImageUploadService
    {
        Task<string> SaveExpenseImageAsync(IFormFile file);
    }
}
