using ExpenseTracker.Api.Data;
using ExpenseTracker.Api.Dtos;
using ExpenseTracker.Api.Dtos.ResponseDtos;
using ExpenseTracker.Api.Interface;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Api.Services
{
    public class DashboardService:IDashboardService
    {
        private readonly ExpenseTrackerDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        public DashboardService(ExpenseTrackerDbContext context,ICurrentUserService currentUserService) {
        
            _context = context;
            _currentUserService = currentUserService;
        }
        //public async Task<DashboardResponse> GetDashboardAsync()
        //{
        //    var dashboard = new DashboardResponse() { 
        //    OpenPaycheckCount=await _context.Paychecks.CountAsync(x=> x.UserId == _currentUserService.UserId && !x.IsClosed),
        //    PaycheckCount=await _context.Paychecks.CountAsync(x=> x.UserId == _currentUserService.UserId),
        //    TotalPlannedIncome=await _context.Paychecks.Where(x=> x.UserId == _currentUserService.UserId).SumAsync(x=> x.Amount),




        //    };

        //    var c= await _context.Paychecks.Where(x => x.UserId == _currentUserService.UserId).Select(x => new PaycheckDetailsResponse()
        //    {
        //        Amount = x.Amount,
        //        ReceivedDate = x.ReceivedDate,
        //        Description = x.Description,
        //        Id = x.Id,
        //        IsClosed = x.IsClosed,

        //        ExpenseCount = x.Expenses.Count,
        //        TotalExpenses = x.Expenses.Sum(x => x.Amount),
        //        RemainingBalance = x.Amount - x.Expenses.Sum(x => x.Amount),
        //        Expenses = x.Expenses.Select(e => new ExpenseResponse
        //        {
        //            Amount = e.Amount,
        //            Description = e.Description,
        //            Id = e.Id,
        //            CategoryId = e.CategoryId,
        //            ExpenseDate = e.ExpenseDate,
        //            PaycheckId = e.PaycheckId,
        //            CategoryName = e.Category != null ? e.Category.Name : "Uncategorized",
        //        }).ToList()

        //    }).ToListAsync();
        //}
    }
}
