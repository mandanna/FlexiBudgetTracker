using ExpenseTracker.Api.Dtos.ResponseDtos.Category;

namespace ExpenseTracker.Api.Dtos
{
    public class DashboardResponse
    {
        public decimal TotalPlannedIncome { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal TotalRemaining { get; set; }
        public int PaycheckCount { get; set; }
        public int OpenPaycheckCount { get; set; }

        public List<PaycheckResponse> UpcomingPaychecks { get; set; } = new();
        public List<ExpenseResponse> RecentExpenses { get; set; } = new();
        public List<CategorySpendingResponse> SpendingByCategory { get; set; } = new();
    }
}
