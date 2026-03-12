using ExpenseWeb.Infrastructure;
using ExpenseWeb.Middlewares;
using ExpenseWeb.Services;
using ExpenseWeb.Services.Contracts;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Extensions.Http;
using System.Net;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// MVC
builder.Services.AddControllersWithViews();

// 🔗 HTTP CLIENT → API (no Azure: Application Setting ServiceUri__ExpenseApi = URL da API)
var apiBaseUrl = builder.Configuration["ServiceUri:ExpenseApi"] ?? "";
if (string.IsNullOrWhiteSpace(apiBaseUrl))
    throw new InvalidOperationException("ServiceUri:ExpenseApi não configurado. No Azure: Application Settings → ServiceUri__ExpenseApi (ex.: https://sua-api.azurewebsites.net)");
builder.Services.AddHttpClient("ExpenseApi", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl.TrimEnd('/') + "/");
    // Reduzido timeout de 300s (usado para cold start no Render) para 30s
    client.Timeout = TimeSpan.FromSeconds(30);
})
.AddPolicyHandler(GetExpenseApiRetryPolicy())
.AddHttpMessageHandler<JwtHandler>(); // Adiciona o token JWT automaticamente em todas as requisições

// 🔐 DATA PROTECTION: em Produção (Render) usa PostgreSQL para chaves sobreviverem a restarts
var dataProtectionConnection = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
    ?? Environment.GetEnvironmentVariable("DATABASE_URL");
dataProtectionConnection = dataProtectionConnection?.Trim();

if (!builder.Environment.IsDevelopment() && !string.IsNullOrWhiteSpace(dataProtectionConnection))
{
    var conn = dataProtectionConnection;
    builder.Services.AddDbContext<DataProtectionDbContext>(options =>
        options.UseNpgsql(conn, npgsql => npgsql.MigrationsHistoryTable("__DataProtectionMigrationsHistory")));

    builder.Services.AddDataProtection()
        .PersistKeysToDbContext<DataProtectionDbContext>()
        .SetApplicationName("ExpenseWeb");
}
else
{
    // Desenvolvimento ou sem DB: chaves em disco (local) ou efêmeras
    var keyPath = Path.Combine(Environment.GetEnvironmentVariable("HOME") ?? "D:\\home", "data", "ProtectionKeys");
    var dir = new DirectoryInfo(keyPath);
    if (!dir.Exists) dir.Create();
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(dir)
        .SetApplicationName("ExpenseWeb");
}

/// <summary>Retry em 429/503/502 e erros transientes.</summary>
static IAsyncPolicy<HttpResponseMessage> GetExpenseApiRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}

// 🔐 SESSION (OBRIGATÓRIO)
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    if (!builder.Environment.IsDevelopment())
    {
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
    }
});

// 🔐 HTTP CONTEXT ACCESSOR (para middleware)
builder.Services.AddHttpContextAccessor();

// 🔐 JWT HANDLER (para adicionar token nas requisições HTTP)
builder.Services.AddTransient<JwtHandler>();

// 🔐 AUTENTICAÇÃO
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
        options.SlidingExpiration = true;
        if (!builder.Environment.IsDevelopment())
        {
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Lax;
        }
    });

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// 🔧 CACHE (categorias pouco alteradas)
builder.Services.AddMemoryCache();

// 🔧 SERVICES
builder.Services.AddScoped<IExpenseService, ExpenseService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<ImageUploadService>();
builder.Services.AddHealthChecks();

var app = builder.Build();

// Criar tabela DataProtectionKeys no PostgreSQL (quando em uso).
// Usamos SQL explícito porque EnsureCreated() não cria tabelas se o banco já existir (ex.: criado pela API).
if (!app.Environment.IsDevelopment())
{
    try
    {
        using var scope = app.Services.CreateScope();
        var dpContext = scope.ServiceProvider.GetService<DataProtectionDbContext>();
        if (dpContext != null)
        {
            await dpContext.Database.ExecuteSqlRawAsync("""
                CREATE TABLE IF NOT EXISTS "DataProtectionKeys" (
                    "Id" SERIAL PRIMARY KEY,
                    "FriendlyName" TEXT NULL,
                    "Xml" TEXT NULL
                );
                """);
        }
    }
    catch
    {
        // Ignorar se não houver connection string (ex.: desenvolvimento sem DB)
    }
}

// 🔄 PIPELINE
app.UseForwardedHeaders();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ⚠️ ORDEM CORRETA
app.UseSession();
app.UseJwtSessionAuthentication(); // Middleware customizado para validar JWT da sessão
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHealthChecks("/health");
app.Run();
