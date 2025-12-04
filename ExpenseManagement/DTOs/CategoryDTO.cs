using ExpenseManagement.Models;
using System.ComponentModel.DataAnnotations;

namespace ExpenseManagement.DTOs
{
    public class CategoryDTO
    {
        
        public int CategoryId { get; set; }
        public required string Name { get; set; }
        public ICollection<Expense>? Expenses { get; set; }
    }
}
