using ExpenseManagement.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Net;
using System.Text;
using Xunit;

namespace ExpenseManagement.Tests.Services
{
    public class ImageUploadServiceTests
    {
        [Fact]
        public async Task SaveExpenseImageAsync_QuandoSupabaseNaoConfigurado_DeveLancarInvalidOperationException()
        {
            var handler = new MockHttpMessageHandler();
            var client = new HttpClient(handler);
            var service = new ImageUploadService(client, "", "", "expense-images");

            var file = CreateMockFormFile("test.jpg", "image/jpeg");

            var act = () => service.SaveExpenseImageAsync(file);
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Supabase*configurado*");
        }

        [Fact]
        public async Task SaveExpenseImageAsync_QuandoUploadSucesso_DeveRetornarUrlPublica()
        {
            var handler = new MockHttpMessageHandler();
            handler.SetupResponse(req =>
            {
                if (req.RequestUri?.ToString().Contains("/storage/v1/object/") == true && req.Method == HttpMethod.Post)
                    return new HttpResponseMessage(HttpStatusCode.OK);
                return null;
            });
            var client = new HttpClient(handler) { BaseAddress = new Uri("https://test.supabase.co") };
            var service = new ImageUploadService(client, "https://test.supabase.co", "fake-anon-key", "expense-images");

            var file = CreateMockFormFile("test.jpg", "image/jpeg");

            var result = await service.SaveExpenseImageAsync(file);

            result.Should().NotBeNullOrEmpty();
            result.Should().StartWith("https://test.supabase.co/storage/v1/object/public/expense-images/expenses/");
            result.Should().EndWith(".jpg");
        }

        [Fact]
        public async Task SaveExpenseImageAsync_QuandoUploadFalha_DeveLancarInvalidOperationException()
        {
            var handler = new MockHttpMessageHandler();
            handler.SetupResponse(req =>
            {
                if (req.RequestUri?.ToString().Contains("/storage/v1/object/") == true)
                    return new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = new StringContent("{\"error\":\"Bad request\"}") };
                return null;
            });
            var client = new HttpClient(handler);
            var service = new ImageUploadService(client, "https://test.supabase.co", "fake-key", "expense-images");

            var file = CreateMockFormFile("test.png", "image/png");

            var act = () => service.SaveExpenseImageAsync(file);
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Falha no upload*");
        }

        [Fact]
        public async Task DeleteExpenseImageAsync_QuandoUrlVazia_DeveRetornarSemErro()
        {
            var handler = new MockHttpMessageHandler();
            var client = new HttpClient(handler);
            var service = new ImageUploadService(client, "https://test.supabase.co", "key", "expense-images");

            await service.Invoking(s => s.DeleteExpenseImageAsync("")).Should().NotThrowAsync();
            await service.Invoking(s => s.DeleteExpenseImageAsync(null!)).Should().NotThrowAsync();
        }

        [Fact]
        public async Task DeleteExpenseImageAsync_QuandoUrlNaoEhSupabase_DeveRetornarSemErro()
        {
            var handler = new MockHttpMessageHandler();
            var client = new HttpClient(handler);
            var service = new ImageUploadService(client, "https://test.supabase.co", "key", "expense-images");

            await service.Invoking(s => s.DeleteExpenseImageAsync("https://other.com/image.jpg")).Should().NotThrowAsync();
        }

        private static IFormFile CreateMockFormFile(string fileName, string contentType)
        {
            var content = "fake image content";
            var formFile = new Mock<IFormFile>();
            formFile.Setup(f => f.FileName).Returns(fileName);
            formFile.Setup(f => f.ContentType).Returns(contentType);
            formFile.Setup(f => f.Length).Returns(content.Length);
            formFile.Setup(f => f.OpenReadStream()).Returns(() => new MemoryStream(Encoding.UTF8.GetBytes(content)));
            return formFile.Object;
        }
    }

    internal class MockHttpMessageHandler : HttpMessageHandler
    {
        private Func<HttpRequestMessage, HttpResponseMessage?>? _responseFactory;

        public void SetupResponse(Func<HttpRequestMessage, HttpResponseMessage?> factory)
        {
            _responseFactory = factory;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = _responseFactory?.Invoke(request) ?? new HttpResponseMessage(HttpStatusCode.NotFound);
            return Task.FromResult(response);
        }
    }
}
