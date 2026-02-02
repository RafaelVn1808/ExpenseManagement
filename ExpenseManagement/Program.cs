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
using Swashbuckle.AspNetCore;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Log de erros de inicialização (aparece no Log Stream do Azure quando habilitado)
try
{
    await RunAsync(builder);
}
catch (Exception ex)
    {
        // HostAbortedException é esperado quando dotnet ef database update encerra o host
        if (ex.GetType().Name != "HostAbortedException")
        {
            await Console.Error.WriteLineAsync($"[STARTUP ERROR] {ex.GetType().Name}: {ex.Message}");
            await Console.Error.WriteLineAsync(ex.StackTrace ?? "");
        }
        throw;
    }

static async Task RunAsync(WebApplicationBuilder builder)
{
// Add services to the container.

// Connection string: appsettings, depois env ConnectionStrings__DefaultConnection, depois DATABASE_URL (Render)
var defaultConnection = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
    ?? Environment.GetEnvironmentVariable("DATABASE_URL");
defaultConnection = defaultConnection?.Trim();
if (string.IsNullOrWhiteSpace(defaultConnection))
{
    throw new InvalidOperationException(
        "ConnectionString não configurada. No Render: Environment → ConnectionStrings__DefaultConnection = Internal Database URL. " +
        "Ou use DATABASE_URL (Render injeta ao vincular o PostgreSQL).");
}

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(defaultConnection, npgsql => 
    {
        npgsql.MigrationsHistoryTable("__AppDbContextMigrationsHistory");
    });
    // Configurar timestamp sem timezone para evitar problemas de UTC
    AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
});

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());  
builder.Services.AddScoped<IExpenseRepository, ExpenseRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IExpenseService, ExpenseService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IImageUploadService, ImageUploadService>();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(defaultConnection, npgsql => 
    {
        npgsql.MigrationsHistoryTable("__ApplicationDbContextMigrationsHistory");
    });
    // Configurar timestamp sem timezone para evitar problemas de UTC
    AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
});


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

// Swagger/OpenAPI (Swashbuckle para .NET 8)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Valida��o da chave JWT
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey) || jwtKey.Length < 32)
{
    throw new InvalidOperationException("Chave JWT não configurada ou com menos de 32 caracteres. No Azure: Application Settings → Jwt__Key (mín. 32 caracteres).");
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
    app.UseSwagger();
    app.UseSwaggerUI(options =>
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Expense Management API"));
}
else
{
    // Em produ��o, proteger Swagger com autentica��o
    app.UseSwagger();
    app.UseSwaggerUI(options =>
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Expense Management API"));
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// CORS deve vir antes de Authentication e Authorization
app.UseCors("AllowExpenseWeb");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

// Executa migrations automaticamente na inicialização
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var appDbContext = services.GetRequiredService<AppDbContext>();
    var appIdentityContext = services.GetRequiredService<ApplicationDbContext>();
    await appDbContext.Database.MigrateAsync();
    await appIdentityContext.Database.MigrateAsync();
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await IdentitySeed.SeedAsync(services, app.Configuration, app.Environment);
}

await app.RunAsync();
}
