using ExpenseManagement.Models;
using System.ComponentModel.DataAnnotations;

namespace ExpenseManagement.DTOs
{
    public class ExpenseDTO
    {
        public int ExpenseId { get; set; }

        // Data da PRIMEIRA parcela
        [Required(ErrorMessage = "A data de início é obrigatória.")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "O nome da despesa é obrigatório.")]
        [MinLength(3, ErrorMessage = "O nome deve ter no mínimo 3 caracteres.")]
        [MaxLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres.")]
        public string? Name { get; set; }

        // Valor TOTAL da despesa
        [Required(ErrorMessage = "O valor total é obrigatório.")]
        [Range(0.01, 999999999.99, ErrorMessage = "O valor total deve estar entre R$ 0,01 e R$ 999.999.999,99.")]
        public decimal TotalAmount { get; set; }

        // Quantidade total de parcelas
        [Range(1, 120, ErrorMessage = "O número de parcelas deve estar entre 1 e 120.")]
        public int Installments { get; set; } = 1;

        // Valor de cada parcela (calculado no service)
        public decimal InstallmentAmount { get; set; }

        // Data limite / validade da despesa (opcional)
        public DateTime? Validity { get; set; }

        [Required(ErrorMessage = "O status é obrigatório.")]
        [EnumDataType(typeof(ExpenseStatus), ErrorMessage = "O status informado é inválido.")]
        public ExpenseStatus Status { get; set; }

        [StringLength(300, ErrorMessage = "A URL da imagem da nota deve ter no máximo 300 caracteres.")]
        public string? NoteImageUrl { get; set; }

        [StringLength(300, ErrorMessage = "A URL da imagem do comprovante deve ter no máximo 300 caracteres.")]
        public string? ProofImageUrl { get; set; }

        [Required(ErrorMessage = "A categoria é obrigatória.")]
        [Range(1, int.MaxValue, ErrorMessage = "O ID da categoria deve ser maior que zero.")]
        public int CategoryId { get; set; }

        // Apenas leitura (retorno da API)
        public string? CategoryName { get; set; }

        // UserId - preenchido automaticamente pelo controller (não deve ser enviado pelo cliente)
        public string? UserId { get; set; }
    }
}
