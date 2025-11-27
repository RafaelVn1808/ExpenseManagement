namespace ExpenseManagement.Models
{
    public class Expense{
        
        public int ExpenseId { get; set; }
        public DateTime Date { get; set; }
        public required string Name { get; set; }
        public required string Category { get; set; }
        public decimal Amount { get; set; }
        public DateTime Validity { get; set; }
        public int Installments { get; set; }
        public required string Status { get; set; }
        public string? NoteImageUrl { get; set; }
        public string? ProofImageUrl { get; set; }






    }
}
