namespace ExpenseWeb.Models;

public class DashboardStatsViewModel
{
    public List<CategorySumViewModel> ByCategory { get; set; } = new();
    public List<MonthSumViewModel> ByMonth { get; set; } = new();
}

public class CategorySumViewModel
{
    public string CategoryName { get; set; } = string.Empty;
    public decimal Total { get; set; }
}

public class MonthSumViewModel
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string Label { get; set; } = string.Empty;
    public decimal Total { get; set; }
}
