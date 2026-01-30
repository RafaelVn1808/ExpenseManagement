using ExpenseWeb.Infrastructure;
using ExpenseWeb.Middlewares;
using ExpenseWeb.Services;
using ExpenseWeb.Services.Contracts;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;

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
})
.AddHttpMessageHandler<JwtHandler>(); // Adiciona o token JWT automaticamente em todas as requisições

// 🔐 DATA PROTECTION (produção: chaves persistentes para sessão/cookies sobreviverem reinício)
if (!builder.Environment.IsDevelopment())
{
    var keyPath = Path.Combine(Environment.GetEnvironmentVariable("HOME") ?? "D:\\home", "data", "ProtectionKeys");
    var dir = new DirectoryInfo(keyPath);
    if (!dir.Exists) dir.Create();
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(dir)
        .SetApplicationName("ExpenseWeb");
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
