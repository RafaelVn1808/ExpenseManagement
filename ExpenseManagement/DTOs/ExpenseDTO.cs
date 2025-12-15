using ExpenseManagement.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ExpenseManagement.DTOs
{
    public class ExpenseDTO
    {

        public int ExpenseId { get; set; }
        [Required(ErrorMessage = "Date is required")]
        public DateTime Date { get; set; }
        [Required(ErrorMessage = "Name is required")]
        [MinLength(3)]
        [MaxLength(100)]
        public required string Name { get; set; }
        [Required(ErrorMessage = "Amount is required")]
        public required decimal Amount { get; set; }
        public DateTime Validity { get; set; }
        public int Installments { get; set; }
        public required string Status { get; set; }
        public string? NoteImageUrl { get; set; }
        public string? ProofImageUrl { get; set; }
        public string? CategoryName { get; set; }
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
    }
}
