using ExpenseTracker.Api.Data;
using ExpenseTracker.Api.Dtos;
using ExpenseTracker.Api.Dtos.ResponseDtos.Category;
using ExpenseTracker.Api.Interface;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Api.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly ExpenseTrackerDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        public DashboardService(ExpenseTrackerDbContext context, ICurrentUserService currentUserService)
        {

            _context = context;
            _currentUserService = currentUserService;
        }
        public async Task<DashboardResponse> GetDashboardAsync(DashboardQueryRequest request)
        {
            var userId = _currentUserService.UserId;
            var today = DateTime.Today;

            var paychecks = _context.Paychecks
                .Where(p => p.UserId == userId);
            if (request.PaycheckId.HasValue)
            {
                paychecks = paychecks.Where(p => p.Id == request.PaycheckId.Value);
            }
            else
            {
                if (request.FromDate.HasValue)
                {
                    paychecks = paychecks.Where(p => p.ReceivedDate >= request.FromDate.Value);
                }
                if (request.ToDate.HasValue)
                {
                    paychecks = paychecks.Where(p => p.ReceivedDate <= request.ToDate.Value);
                }
            }
            var scopedIds = paychecks.Select(p => p.Id);
            var settledIds = paychecks.Where(p => p.IsClosed).Select(p => p.Id);

            var expenses = _context.Expenses
                .Where(e => scopedIds.Contains(e.PaycheckId));

            var settledIncome = await _context.Incomes
                .Where(i => settledIds.Contains(i.PaycheckId))
                .SumAsync(i => (decimal?)i.Amount) ?? 0m;

            var settledSpent = await _context.Expenses
                .Where(e => settledIds.Contains(e.PaycheckId))
                .SumAsync(e => (decimal?)e.Amount) ?? 0m;

            var settledCount = await paychecks.CountAsync(p => p.IsClosed);

            var currentPaycheck = _context.Paychecks
                .Where(x => !x.IsClosed)
                .OrderBy(x => x.ReceivedDate)
                .Select(p => new CurrentPaycheckResponse
                {
                    Description = p.Description,
                    Id = p.Id,
                    ReceivedDate = p.ReceivedDate,
                    TotalIncome = p.Incomes.Sum(i => i.Amount),
                    TotalSpent = p.Expenses.Sum(e => e.Amount),
                    ProjectedSavings = p.Incomes.Sum(i => i.Amount) - p.Expenses.Sum(e => e.Amount),
                    ExpenseCount = p.Expenses.Count
                }).FirstOrDefault();


            var recentExpenses = await expenses
                .OrderByDescending(e => e.ExpenseDate)
                .Take(5)
                .Select(e => new ExpenseResponse
                {
                    Id = e.Id,
                    Description = e.Description,
                    Amount = e.Amount,
                    ExpenseDate = e.ExpenseDate,
                    PaycheckId = e.PaycheckId,
                    CategoryId = e.CategoryId,
                    CategoryName = e.Category != null ? e.Category.Name : "Uncategorized"
                })
                .ToListAsync();

            var spendingByCategory = await expenses
                .GroupBy(e => new { e.CategoryId, CategoryName = e.Category != null ? e.Category.Name : "Uncategorized" })
                .Select(g => new CategorySpendingResponse
                {
                    CategoryId = g.Key.CategoryId,
                    CategoryName = g.Key.CategoryName,
                    TotalAmount = g.Sum(e => e.Amount),
                    ExpenseCount = g.Count()
                })
                .OrderByDescending(c => c.TotalAmount)
                .ToListAsync();
            return new DashboardResponse
            {
                SettledIncome = settledIncome,
                SettledSpent = settledSpent,
                SettledSavings = settledIncome - settledSpent,
                SettledPaycheckCount = settledCount,
                CurrentPaycheck = currentPaycheck,
                RecentExpenses = recentExpenses,
                SpendingByCategory = spendingByCategory
            };
        }
    }
}
