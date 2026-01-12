using System.Text.Json.Serialization;

namespace ExpenseWeb.Models
{
    public class LoginResponse
    {
        [JsonPropertyName("token")]
        public string Token { get; set; }
    }
}
