using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json;

namespace ExpenseWeb.Middlewares
{
    public class JwtSessionAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public JwtSessionAuthenticationMiddleware(
            RequestDelegate next,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory)
        {
            _next = next;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var token = context.Session.GetString("JWToken");

            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key não configurada"));

                    var validationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = true,
                        ValidIssuer = _configuration["Jwt:Issuer"],
                        ValidateAudience = true,
                        ValidAudience = _configuration["Jwt:Audience"],
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };

                    var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

                    if (principal != null)
                    {
                        context.User = principal;
                    }
                }
                catch (SecurityTokenExpiredException)
                {
                    var refreshed = await TryRefreshTokenAsync(context);
                    if (refreshed)
                    {
                        var refreshedToken = context.Session.GetString("JWToken");
                        if (!string.IsNullOrEmpty(refreshedToken))
                        {
                            var tokenHandler = new JwtSecurityTokenHandler();
                            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key não configurada"));

                            var validationParameters = new TokenValidationParameters
                            {
                                ValidateIssuerSigningKey = true,
                                IssuerSigningKey = new SymmetricSecurityKey(key),
                                ValidateIssuer = true,
                                ValidIssuer = _configuration["Jwt:Issuer"],
                                ValidateAudience = true,
                                ValidAudience = _configuration["Jwt:Audience"],
                                ValidateLifetime = true,
                                ClockSkew = TimeSpan.Zero
                            };

                            var principal = tokenHandler.ValidateToken(refreshedToken, validationParameters, out SecurityToken validatedToken);
                            if (principal != null)
                            {
                                context.User = principal;
                            }
                        }
                    }
                    else
                    {
                        context.Session.Remove("JWToken");
                        context.Session.Remove("RefreshToken");
                        context.Session.Remove("JwtExpiresAt");
                    }
                }
                catch
                {
                    // Token inválido ou expirado - limpar sessão
                    context.Session.Remove("JWToken");
                    context.Session.Remove("RefreshToken");
                    context.Session.Remove("JwtExpiresAt");
                }
            }

            await _next(context);
        }

        private async Task<bool> TryRefreshTokenAsync(HttpContext context)
        {
            var refreshToken = context.Session.GetString("RefreshToken");
            if (string.IsNullOrWhiteSpace(refreshToken))
                return false;

            var client = _httpClientFactory.CreateClient("ExpenseApi");
            var payload = JsonSerializer.Serialize(new { refreshToken });
            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("api/auth/refresh", content);
            if (!response.IsSuccessStatusCode)
                return false;

            var json = await response.Content.ReadAsStringAsync();
            var loginResponse = JsonSerializer.Deserialize<ExpenseWeb.Models.LoginResponse>(json);

            if (loginResponse == null || string.IsNullOrEmpty(loginResponse.Token))
                return false;

            context.Session.SetString("JWToken", loginResponse.Token);
            context.Session.SetString("RefreshToken", loginResponse.RefreshToken);
            context.Session.SetString("JwtExpiresAt", loginResponse.ExpiresAt.ToUniversalTime().ToString("O"));

            return true;
        }
    }

    public static class JwtSessionAuthenticationMiddlewareExtensions
    {
        public static IApplicationBuilder UseJwtSessionAuthentication(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<JwtSessionAuthenticationMiddleware>();
        }
    }
}
