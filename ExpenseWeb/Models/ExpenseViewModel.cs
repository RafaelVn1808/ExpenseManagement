using System.ComponentModel.DataAnnotations;

namespace ExpenseWeb.Models
{
    public class ExpenseViewModel
    {
        public int ExpenseId { get; set; }

        [Required]
        public DateTime Date { get; set; } // data base (início da despesa)

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public decimal Amount { get; set; }

        public DateTime Validity { get; set; }

        public int Installments { get; set; } // total de parcelas

        [Required]
        public string Status { get; set; } = string.Empty;

        public string? NoteImageUrl { get; set; }
        public string? ProofImageUrl { get; set; }

        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }

        // 🔽 NOVOS CAMPOS 🔽

        public int CurrentInstallment { get; set; } // 1, 2, 3...
        public string InstallmentLabel =>
            Installments > 1 ? $"{CurrentInstallment}/{Installments}" : "À vista";

        public int ReferenceMonth { get; set; } // mês exibido (1–12)
        public int ReferenceYear { get; set; }  // ano exibido
    }
}
