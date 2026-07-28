using ExpenseTracker.Api.Dtos.ResponseDtos.Category;

namespace ExpenseTracker.Api.Dtos
{
    public class DashboardResponse
    {

        public decimal SettledIncome { get; set; }
        public decimal SettledSpent { get; set; }
        public decimal SettledSavings { get; set; }
        public int SettledPaycheckCount { get; set; }

        public CurrentPaycheckResponse? CurrentPaycheck { get; set; }
        public List<ExpenseResponse> RecentExpenses { get; set; } = new();
        public List<CategorySpendingResponse> SpendingByCategory { get; set; } = new();
    }
}
