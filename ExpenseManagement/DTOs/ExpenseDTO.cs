using ExpenseManagement.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ExpenseManagement.DTOs
{
    public class ExpenseDTO
    {
        public int ExpenseId { get; set; }

        // Data da PRIMEIRA parcela
        [Required(ErrorMessage = "Start date is required")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [MinLength(3)]
        [MaxLength(100)]
        public string? Name { get; set; }

        // Valor TOTAL da despesa
        [Required(ErrorMessage = "Total amount is required")]
        public decimal TotalAmount { get; set; }

        // Quantidade total de parcelas
        [Range(1, 120)]
        public int Installments { get; set; }

        // Valor de cada parcela (calculado no service)
        public decimal InstallmentAmount { get; set; }

        public DateTime? Validity { get; set; }

        [Required]
        public string? Status { get; set; }

        public string? NoteImageUrl { get; set; }
        public string? ProofImageUrl { get; set; }

        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
    }

}
