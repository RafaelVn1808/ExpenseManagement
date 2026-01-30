using AutoMapper;
using ExpenseApi.Context;
using ExpenseApi.Identity;
using ExpenseApi.Middlewares;
using ExpenseManagement.Context;
using ExpenseManagement.Repositories;
using ExpenseManagement.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Validação: connection string em appsettings.json / appsettings.Development.json (produção: Azure App Settings)
var defaultConnection = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(defaultConnection))
{
    throw new InvalidOperationException(
        "ConnectionString 'DefaultConnection' não configurada. Configure em appsettings.json ou appsettings.Development.json (produção: Azure App Settings).");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(defaultConnection));

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());  
builder.Services.AddScoped<IExpenseRepository, ExpenseRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IExpenseService, ExpenseService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IImageUploadService, ImageUploadService>();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(defaultConnection));


builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        // Requisitos de senha mais fortes
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredUniqueChars = 1;

        // Configura��es de lockout para prote��o contra brute force
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;

        // Configura��es de usu�rio
        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedEmail = false; // Pode ser habilitado em produ��o
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddMemoryCache();

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowExpenseWeb", policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new[] { "https://localhost:7000" })
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Valida��o da chave JWT
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey) || jwtKey.Length < 32)
{
    throw new InvalidOperationException("Chave JWT não configurada ou com menos de 32 caracteres. Configure em appsettings.json ou appsettings.Development.json (produção: Azure App Settings).");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.Zero, // Remove toler�ncia de tempo para tokens expirados

        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtKey))
    };
    
    // Eventos para logging de segurança
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            // Log de falha de autenticação pode ser adicionado aqui
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            // Validações adicionais podem ser feitas aqui
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddHealthChecks();



var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();
// Configure the HTTP request pipeline.
app.UseForwardedHeaders();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
        options.SwaggerEndpoint("/openapi/v1.json", "Expense Management API"));
}
else
{
    // Em produ��o, proteger Swagger com autentica��o
    app.MapOpenApi().RequireAuthorization();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// CORS deve vir antes de Authentication e Authorization
app.UseCors("AllowExpenseWeb");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await IdentitySeed.SeedAsync(services, app.Configuration, app.Environment);
}


app.Run();
