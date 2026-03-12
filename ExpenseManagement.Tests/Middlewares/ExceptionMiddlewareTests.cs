using ExpenseApi.ErrorResponse;
using ExpenseApi.Middlewares;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text;
using System.Text.Json;
using Xunit;

namespace ExpenseManagement.Tests.Middlewares
{
    public class ExceptionMiddlewareTests
    {
        private static (ExceptionMiddleware Middleware, HttpContext Context) CreateMiddleware(
            IHostEnvironment env,
            RequestDelegate? next = null)
        {
            var logger = new Mock<ILogger<ExceptionMiddleware>>();
            var nextDelegate = next ?? (ctx => Task.CompletedTask);
            var middleware = new ExceptionMiddleware(nextDelegate, logger.Object, env);

            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();
            context.TraceIdentifier = "trace-123";

            return (middleware, context);
        }

        [Fact]
        public async Task InvokeAsync_QuandoBusinessException_DeveRetornar400()
        {
            var env = new Mock<IHostEnvironment>();
            env.Setup(e => e.EnvironmentName).Returns("Production");

            RequestDelegate next = _ => throw new BusinessException("Erro de negócio");

            var (middleware, context) = CreateMiddleware(env.Object, next);

            await middleware.InvokeAsync(context);

            context.Response.StatusCode.Should().Be(400);
            var body = await ReadResponseBody(context);
            var json = JsonSerializer.Deserialize<JsonElement>(body);
            json.GetProperty("message").GetString().Should().Be("Erro de negócio");
            json.GetProperty("statusCode").GetInt32().Should().Be(400);
        }

        [Fact]
        public async Task InvokeAsync_QuandoUnauthorizedAccessException_DeveRetornar401()
        {
            var env = new Mock<IHostEnvironment>();
            env.Setup(e => e.EnvironmentName).Returns("Production");

            RequestDelegate next = _ => throw new UnauthorizedAccessException("Não autorizado");

            var (middleware, context) = CreateMiddleware(env.Object, next);

            await middleware.InvokeAsync(context);

            context.Response.StatusCode.Should().Be(401);
            var body = await ReadResponseBody(context);
            var json = JsonSerializer.Deserialize<JsonElement>(body);
            json.GetProperty("statusCode").GetInt32().Should().Be(401);
        }

        [Fact]
        public async Task InvokeAsync_QuandoExcecaoGenerica_DeveRetornar500()
        {
            var env = new Mock<IHostEnvironment>();
            env.Setup(e => e.EnvironmentName).Returns("Production");

            RequestDelegate next = _ => throw new InvalidOperationException("Erro inesperado");

            var (middleware, context) = CreateMiddleware(env.Object, next);

            await middleware.InvokeAsync(context);

            context.Response.StatusCode.Should().Be(500);
            var body = await ReadResponseBody(context);
            var json = JsonSerializer.Deserialize<JsonElement>(body);
            json.GetProperty("message").GetString().Should().Be("Ocorreu um erro inesperado.");
            json.GetProperty("statusCode").GetInt32().Should().Be(500);
        }

        [Fact]
        public async Task InvokeAsync_EmDevelopment_DeveIncluirDetails()
        {
            var env = new Mock<IHostEnvironment>();
            env.Setup(e => e.EnvironmentName).Returns("Development");

            RequestDelegate next = _ => throw new BusinessException("Teste");

            var (middleware, context) = CreateMiddleware(env.Object, next);

            await middleware.InvokeAsync(context);

            var body = await ReadResponseBody(context);
            var json = JsonSerializer.Deserialize<JsonElement>(body);
            json.TryGetProperty("details", out var details).Should().BeTrue();
            details.GetString().Should().Contain("BusinessException");
        }

        private static async Task<string> ReadResponseBody(HttpContext context)
        {
            context.Response.Body.Position = 0;
            using var reader = new StreamReader(context.Response.Body, Encoding.UTF8);
            return await reader.ReadToEndAsync();
        }
    }
}
