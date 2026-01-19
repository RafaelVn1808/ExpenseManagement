using System.ComponentModel.DataAnnotations;

namespace ExpenseApi.Models
{
    public class RefreshTokenRequestDto
    {
        [Required(ErrorMessage = "O refresh token é obrigatório.")]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
