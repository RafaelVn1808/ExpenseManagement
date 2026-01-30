using System.ComponentModel.DataAnnotations;

namespace ExpenseApi.Models
{
    public class ForgotPasswordDto
    {
        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
        public string Email { get; set; } = string.Empty;
    }
}
