using ExpenseTracker.Api.Data;
using ExpenseTracker.Api.Dtos;
using ExpenseTracker.Api.Dtos.RequestDtos.QueryRequest.Paycheck;
using ExpenseTracker.Api.Dtos.ResponseDtos;
using ExpenseTracker.Api.Exceptions;
using ExpenseTracker.Api.Interface;
using ExpenseTracker.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Api.Services
{
    public class PaycheckService : IPaycheckService
    {
        private readonly ExpenseTrackerDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<PaycheckService> _logger;
        public PaycheckService(ExpenseTrackerDbContext context, ICurrentUserService currentUserService, ILogger<PaycheckService> logger)
        {
            _context = context;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<bool> ClosePaycheck(int paycheckId, bool toggle)
        {
            var paycheck = await _context.Paychecks.FirstOrDefaultAsync(x => x.Id == paycheckId && x.UserId == _currentUserService.UserId);
            if (paycheck == null)
                return false;

            paycheck.IsClosed = toggle;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Paycheck with ID {PaycheckId} has been {Action}.", paycheckId, toggle ? "closed" : "opened");
            return true;
        }

        public async Task<PaycheckResponse> CreatePaycheckAsync(CreatePaycheckRequest request)
        {
            var paycheck = new Paycheck
            {
                Description = request.Description,
                Amount = request.Amount,
                ReceivedDate = request.ReceivedDate,
                IsClosed = false,
                UserId = _currentUserService.UserId
            };

            _context.Paychecks.Add(paycheck);

            await _context.SaveChangesAsync();
            _logger.LogInformation("Paycheck with ID {PaycheckId} has been created.", paycheck.Id);

            return new PaycheckResponse
            {
                Amount = paycheck.Amount,
                Description = paycheck.Description,
                Id = paycheck.Id,
                ReceivedDate = paycheck.ReceivedDate,
                IsClosed = paycheck.IsClosed
            };
        }
        public async Task<PaycheckResponse> GetPaycheckAsync(int id)
        {
            var paycheck = await _context.Paychecks.FirstOrDefaultAsync(x => x.Id == id && x.UserId == _currentUserService.UserId);
            if (paycheck == null) throw new NotFoundException("Paycheck not found.");

            return new PaycheckResponse
            {
                Amount = paycheck.Amount,
                Description = paycheck.Description,
                Id = paycheck.Id,
                ReceivedDate = paycheck.ReceivedDate,
                IsClosed = paycheck.IsClosed
            };
        }

        public async Task<List<PaycheckDetailsResponse>> GetDashboardAsync()
        {
            return await _context.Paychecks.Where(x=>x.UserId==_currentUserService.UserId).Select(x => new PaycheckDetailsResponse()
            {
                Amount = x.Amount,
                ReceivedDate = x.ReceivedDate,
                Description = x.Description,
                Id = x.Id,
                IsClosed = x.IsClosed,

                ExpenseCount = x.Expenses.Count,
                TotalExpenses = x.Expenses.Sum(x => x.Amount),
                RemainingBalance = x.Amount - x.Expenses.Sum(x => x.Amount),
                Expenses = x.Expenses.Select(e => new ExpenseResponse
                {
                    Amount = e.Amount,
                    Description = e.Description,
                    Id = e.Id,
                    CategoryId = e.CategoryId,
                    ExpenseDate = e.ExpenseDate,
                    PaycheckId = e.PaycheckId,
                    CategoryName = e.Category != null ? e.Category.Name : "Uncategorized",
                }).ToList()

            }).ToListAsync();
        }

        public async Task<List<PaycheckResponse>> GetPaychecksAsync()
        {
            var paychecks = await _context.Paychecks.Where(x => x.UserId == _currentUserService.UserId).Select(x => new PaycheckResponse
            {
                Id = x.Id,
                Amount = x.Amount,
                Description = x.Description,
                ReceivedDate = x.ReceivedDate,
                IsClosed = x.IsClosed
            }).ToListAsync(); ;

            return paychecks;
            // if (request.IsClosed.HasValue) { 

            //paychecks=paychecks.Where(x => x.IsClosed == request.IsClosed.Value);
            // }
            // if (!string.IsNullOrWhiteSpace(request.Search)) {
            //     paychecks = paychecks.Where(x =>x.Description.Contains(request.Search));
            // }


            //    await paychecks.Select(x => new PaycheckResponse
            //    {
            //        Id = x.Id,
            //        Amount = x.Amount,
            //        Description = x.Description,
            //        ReceivedDate = x.ReceivedDate,
            //        IsClosed = x.IsClosed
            //    }).ToListAsync();

            //    return new PagedResponse<PaycheckResponse> { };
        }

        public async Task<PaycheckSummaryResponse?> GetSummaryAsync(int paycheckId)
        {
            var paycheck = await _context.Paychecks.Include(x => x.Expenses).FirstOrDefaultAsync(x => x.Id == paycheckId && x.UserId==_currentUserService.UserId);
            if (paycheck == null) return null;
            var totalExpense = paycheck.Expenses.Sum(x => x.Amount);
            return new PaycheckSummaryResponse
            {
                PaycheckAmount = paycheck.Amount,
                TotalExpenses = totalExpense,
                RemainingBalance = paycheck.Amount - totalExpense,
                ExpenseCount = paycheck.Expenses.Count,
                IsClosed = paycheck.IsClosed
            };


        }
    }
}