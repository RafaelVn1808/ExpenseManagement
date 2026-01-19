using System.ComponentModel.DataAnnotations;

namespace ExpenseWeb.Models
{
    public class CategoryViewModel
    {
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "O nome da categoria é obrigatório.")]
        [MinLength(2, ErrorMessage = "O nome deve ter no mínimo 2 caracteres.")]
        [MaxLength(80, ErrorMessage = "O nome deve ter no máximo 80 caracteres.")]
        [Display(Name = "Nome da Categoria")]
        public required string Name { get; set; }
    }
}
