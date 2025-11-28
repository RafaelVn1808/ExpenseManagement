using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpenseManagement.Models
{
    public class Expense{

        [Key]
        public int ExpenseId { get; set; }
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public required string Name { get; set; }
        [Required]
        [Column(TypeName ="decimal(10,2)")]
        public required decimal Amount { get; set; }
        public DateTime Validity { get; set; }
        
        public int Installments { get; set; }
        [Required]

        [StringLength(20)]
        public required string Status { get; set; }
        [StringLength(300)]
        public string? NoteImageUrl { get; set; }
        [StringLength(300)]
        public string? ProofImageUrl { get; set; }
        public Category? Category { get; set; }





    }
}
