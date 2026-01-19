namespace ExpenseWeb.Models
{
    public class ExpenseQueryParameters
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public int? CategoryId { get; set; }
        public string? Status { get; set; }
        public string? Search { get; set; }
        public string? SortBy { get; set; }
        public string? SortDir { get; set; } = "desc";
    }
}
