using System.ComponentModel.DataAnnotations;

namespace ExpenseManagement.DTOs
{
    public class CategoryDTO
    {
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "O nome da categoria é obrigatório")]
        [StringLength(50, MinimumLength = 3,
            ErrorMessage = "O nome da categoria deve ter entre 3 e 50 caracteres")]
        public string Name { get; set; } = string.Empty;
    }
}
