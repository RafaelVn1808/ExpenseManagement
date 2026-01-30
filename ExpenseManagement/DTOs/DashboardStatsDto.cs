namespace ExpenseManagement.DTOs;

public class DashboardStatsDto
{
    public List<CategorySumDto> ByCategory { get; set; } = new();
    public List<MonthSumDto> ByMonth { get; set; } = new();
}

public class CategorySumDto
{
    public string CategoryName { get; set; } = string.Empty;
    public decimal Total { get; set; }
}

public class MonthSumDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string Label { get; set; } = string.Empty;
    public decimal Total { get; set; }
}
