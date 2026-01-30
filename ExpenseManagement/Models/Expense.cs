using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ExpenseManagement.Models
{
    public class Expense
    {
        [Key]
        public int ExpenseId { get; set; }

        // Data da PRIMEIRA parcela
        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public string? Name { get; set; }

        // Valor TOTAL da despesa
        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalAmount { get; set; }

        // Quantidade de parcelas
        public int Installments { get; set; }

        // Valor de CADA parcela
        [Column(TypeName = "decimal(10,2)")]
        public decimal InstallmentAmount { get; set; }

        public DateTime? Validity { get; set; }

        [Required]
        public ExpenseStatus Status { get; set; }

        [StringLength(300)]
        public string? NoteImageUrl { get; set; }

        [StringLength(300)]
        public string? ProofImageUrl { get; set; }

        public int CategoryId { get; set; }

        [JsonIgnore]
        public Category? Category { get; set; }

        // Relacionamento com usuário
        [Required]
        [StringLength(450)] // Tamanho padrão do ID do Identity
        public string UserId { get; set; } = string.Empty;

        /// <summary>Data/hora de criação do registro (auditoria).</summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>Data/hora da última alteração (auditoria).</summary>
        public DateTime? UpdatedAt { get; set; }
    }
}
