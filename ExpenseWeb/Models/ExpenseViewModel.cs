using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

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

        [Range(1, 120, ErrorMessage = "O número de parcelas deve estar entre 1 e 120.")]
        public int Installments { get; set; } = 1; // total de parcelas

        [Required(ErrorMessage = "O status é obrigatório.")]
        public string Status { get; set; } = string.Empty;

        public string? NoteImageUrl { get; set; }
        public string? ProofImageUrl { get; set; }

        // Propriedades para upload de arquivos (não são enviadas para a API)
        [Display(Name = "Imagem da Nota Fiscal")]
        public IFormFile? NoteImageFile { get; set; }

        [Display(Name = "Imagem do Comprovante")]
        public IFormFile? ProofImageFile { get; set; }

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
