using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace ExpenseManagement.Models
{
    public class Category
    {
        public Category()
        {
            Expenses = new Collection<Expense>();
        }
        [Key]
        public int CategoryId { get; set; }
        [Required]
        [StringLength(80)]
        public required string Name { get; set; }

        public ICollection<Expense>? Expenses { get; set; }

    }
}
