using System.ComponentModel.DataAnnotations;

namespace ExpenseManagement.DTOs
{
    public class CategoryDTO
    {
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "O nome da categoria é obrigatório.")]
        [MinLength(2, ErrorMessage = "O nome deve ter no mínimo 2 caracteres.")]
        [MaxLength(80, ErrorMessage = "O nome deve ter no máximo 80 caracteres.")]
        public required string Name { get; set; }

        public ICollection<Expense>? Expenses { get; set; }
    }
}
