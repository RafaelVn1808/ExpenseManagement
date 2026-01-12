using System.Net.Http.Headers;

namespace ExpenseWeb.Infrastructure
{
    public class JwtHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _accessor;

        public JwtHandler(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var token = _accessor.HttpContext?
                .Session.GetString("JWToken");

            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
