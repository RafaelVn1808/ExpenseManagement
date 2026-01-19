using ExpenseWeb.Infrastructure;
using ExpenseWeb.Middlewares;
using ExpenseWeb.Services;
using ExpenseWeb.Services.Contracts;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// MVC
builder.Services.AddControllersWithViews();

// 🔗 HTTP CLIENT → API
builder.Services.AddHttpClient("ExpenseApi", client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["ServiceUri:ExpenseApi"]!
    );
})
.AddHttpMessageHandler<JwtHandler>(); // Adiciona o token JWT automaticamente em todas as requisições

// 🔐 SESSION (OBRIGATÓRIO)
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
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
    });

// 🔧 SERVICES
builder.Services.AddScoped<IExpenseService, ExpenseService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ImageUploadService>();

var app = builder.Build();

// 🔄 PIPELINE
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

app.Run();
