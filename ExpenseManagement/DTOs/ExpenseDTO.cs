using System.ComponentModel.DataAnnotations;

namespace ExpenseManagement.DTOs
{
    public class ExpenseDTO
    {
        public int ExpenseId { get; set; }

        // Data da PRIMEIRA parcela
        [Required(ErrorMessage = "A data inicial é obrigatória")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(100, MinimumLength = 3,
            ErrorMessage = "O nome deve ter entre 3 e 100 caracteres")]
        public string Name { get; set; } = string.Empty;

        // Valor TOTAL da despesa
        [Required(ErrorMessage = "O valor total é obrigatório")]
        [Range(0.01, double.MaxValue,
            ErrorMessage = "O valor total deve ser maior que zero")]
        public decimal TotalAmount { get; set; }

        // Quantidade total de parcelas
        [Range(1, 120, ErrorMessage = "O número de parcelas deve estar entre 1 e 120")]
        public int Installments { get; set; } = 1;

        // Valor de cada parcela (calculado no service)
        public decimal InstallmentAmount { get; set; }

        // Data limite / validade da despesa (opcional)
        public DateTime? Validity { get; set; }

        [Required(ErrorMessage = "O status é obrigatório")]
        [StringLength(20, ErrorMessage = "Status inválido")]
        public string Status { get; set; } = string.Empty;

        [Url(ErrorMessage = "URL da nota fiscal inválida")]
        public string? NoteImageUrl { get; set; }

        [Url(ErrorMessage = "URL do comprovante inválida")]
        public string? ProofImageUrl { get; set; }

        [Required(ErrorMessage = "Categoria é obrigatória")]
        public int CategoryId { get; set; }

        // Apenas leitura (retorno da API)
        public string? CategoryName { get; set; }
    }
}
