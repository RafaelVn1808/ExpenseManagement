using System.ComponentModel.DataAnnotations;

namespace ExpenseWeb.Models
{
    public class CategoryViewModel
    {
        public int CategoryId { get; set; }
        public required string Name { get; set; }
    }
}
