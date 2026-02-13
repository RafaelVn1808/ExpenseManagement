namespace ExpenseWeb.Services;

/// <summary>
/// Lançada quando a API retorna 429 (Too Many Requests) ou 503 (Service Unavailable),
/// por exemplo durante cold start no Render.
/// </summary>
public class ApiUnavailableException : Exception
{
    public ApiUnavailableException() : base("A API está temporariamente indisponível.")
    {
    }

    public ApiUnavailableException(string message) : base(message)
    {
    }
}
