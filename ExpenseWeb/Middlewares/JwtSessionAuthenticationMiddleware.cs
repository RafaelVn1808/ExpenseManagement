using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace ExpenseWeb.Middlewares
{
    public class JwtSessionAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;

        public JwtSessionAuthenticationMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
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
                catch
                {
                    // Token inválido ou expirado - limpar sessão
                    context.Session.Remove("JWToken");
                }
            }

            await _next(context);
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
