using System.Text.Json.Serialization;

namespace ExpenseWeb.Services
{
    // DTO para comunicação com a API (formato ExpenseDTO)
    public class ExpenseApiDto
    {
        [JsonPropertyName("expenseId")]
        public int ExpenseId { get; set; }

        [JsonPropertyName("startDate")]
        public DateTime StartDate { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("totalAmount")]
        public decimal TotalAmount { get; set; }

        [JsonPropertyName("installments")]
        public int Installments { get; set; }

        [JsonPropertyName("installmentAmount")]
        public decimal InstallmentAmount { get; set; }

        [JsonPropertyName("validity")]
        public DateTime? Validity { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("noteImageUrl")]
        public string? NoteImageUrl { get; set; }

        [JsonPropertyName("proofImageUrl")]
        public string? ProofImageUrl { get; set; }

        [JsonPropertyName("categoryId")]
        public int CategoryId { get; set; }

        [JsonPropertyName("categoryName")]
        public string? CategoryName { get; set; }
    }
}
