namespace ExpenseWeb.Models
{
    public class ExpenseViewModel
    {
        public int ExpenseId { get; set; }
        
        public DateTime Date { get; set; }
        
        public required string Name { get; set; }   
        public required decimal Amount { get; set; }
        public DateTime Validity { get; set; }
        public int Installments { get; set; }
        public required string Status { get; set; }
        public string? NoteImageUrl { get; set; }
        public string? ProofImageUrl { get; set; }
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
    }
}
