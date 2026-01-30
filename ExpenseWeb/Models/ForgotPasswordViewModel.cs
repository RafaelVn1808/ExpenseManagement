using System.ComponentModel.DataAnnotations;

namespace ExpenseWeb.Models
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Informe o e-mail.")]
        [EmailAddress(ErrorMessage = "E-mail inválido.")]
        [Display(Name = "E-mail")]
        public string Email { get; set; } = string.Empty;
    }
}
