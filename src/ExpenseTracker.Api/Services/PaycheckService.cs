using ExpenseTracker.Api.Data;
using ExpenseTracker.Api.Dtos;
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
                ReceivedDate = request.ReceivedDate?? DateTime.Now,
                IsClosed = false,
                UserId = _currentUserService.UserId,               
            };

            if (request.IncomesToCreate is not null)
            {
                foreach (var income in request.IncomesToCreate)
                {
                    paycheck.Incomes.Add(new Income
                    {
                        Source = income.Source,
                        Amount = income.Amount,
                        ReceivedDate = income.ReceivedDate ?? DateTime.Now,
                    });
                }
            }

            _context.Paychecks.Add(paycheck);

            await _context.SaveChangesAsync();
            _logger.LogInformation("Paycheck with ID {PaycheckId} has been created.", paycheck.Id);

            return new PaycheckResponse
            {
                Id = paycheck.Id,
                TotalIncome = paycheck.Incomes.Sum(x=>x.Amount),
                Description = paycheck.Description,
                ReceivedDate = paycheck.ReceivedDate,
                IsClosed = paycheck.IsClosed
            };
        }
        public async Task<PaycheckResponse> GetPaycheckAsync(int id)
        {
            var paycheck = await _context.Paychecks.FirstOrDefaultAsync(x => x.Id == id && x.UserId == _currentUserService.UserId);
            if (paycheck == null) throw new NotFoundException("Paycheck not found.");

            var totalIncome = await _context.Incomes.Where(i => i.PaycheckId == id).SumAsync(i => i.Amount);
            return new PaycheckResponse
            {
                TotalIncome = totalIncome,
                Description = paycheck.Description,
                Id = paycheck.Id,
                ReceivedDate = paycheck.ReceivedDate,
                IsClosed = paycheck.IsClosed
            };
        }

        

        public async Task<PagedResponse<PaycheckResponse>> GetPaychecksAsync(PaycheckQueryRequest request)
        {
            IQueryable<Paycheck> query = _context.Paychecks
                .Where(x => x.UserId == _currentUserService.UserId);

            if (request.IsClosed.HasValue)
            {
                query = query.Where(x => x.IsClosed == request.IsClosed.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim();
                query = query.Where(x => x.Description.Contains(search));
            }

            query = ApplySort(query, request.SortBy, request.Descending);

            var totalRecords = await query.CountAsync();

            // The validator already enforces these bounds over HTTP; clamping here keeps the
            // method safe for any caller that bypasses the request pipeline.
            var page = Math.Max(1, request.Page);
            var pageSize = Math.Clamp(request.PageSize, 1, 100);

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new PaycheckResponse
                {
                    Id = x.Id,
                    TotalIncome = x.Incomes.Sum(i => i.Amount),
                    Description = x.Description,
                    ReceivedDate = x.ReceivedDate,
                    IsClosed = x.IsClosed
                })
                .ToListAsync();

            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            _logger.LogInformation(
                "Retrieved {Count} paycheck(s) (page {Page} of {TotalPages}, {TotalRecords} total) for user {UserId}.",
                items.Count, page, totalPages, totalRecords, _currentUserService.UserId);

            return new PagedResponse<PaycheckResponse>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                TotalPages = totalPages
            };
        }

        private static IQueryable<Paycheck> ApplySort(IQueryable<Paycheck> query, string? sortBy, bool descending)
        {
            // A whitelist switch rather than reflection or dynamic LINQ: every branch is a
            // compile-time-checked property access, so no user-supplied string can reach an
            // unintended property. Unrecognised values are rejected by the validator before
            // they get here; the default branch orders by newest paycheck first.
            return sortBy?.Trim().ToLowerInvariant() switch
            {
                "amount" => descending ? query.OrderByDescending(x => x.Incomes.Sum(i => i.Amount)) : query.OrderBy(x => x.Incomes.Sum(i => i.Amount)),
                "description" => descending ? query.OrderByDescending(x => x.Description) : query.OrderBy(x => x.Description),
                "isclosed" => descending ? query.OrderByDescending(x => x.IsClosed) : query.OrderBy(x => x.IsClosed),
                "receiveddate" => descending ? query.OrderByDescending(x => x.ReceivedDate) : query.OrderBy(x => x.ReceivedDate),
                _ => query.OrderByDescending(x => x.ReceivedDate)
            };
        }

        public async Task<PaycheckSummaryResponse?> GetSummaryAsync(int paycheckId)
        {
            var paycheck = await _context.Paychecks
                .Include(x => x.Expenses)
                .Include(x=>x.Incomes)
                .FirstOrDefaultAsync(x => x.Id == paycheckId && x.UserId==_currentUserService.UserId);
            if (paycheck == null) return null;
            var totalIncome = paycheck.Incomes.Sum(x => x.Amount);
            var totalExpense = paycheck.Expenses.Sum(x => x.Amount);
            return new PaycheckSummaryResponse
            {
                PaycheckAmount = totalIncome,
                TotalExpenses = totalExpense,
                RemainingBalance = totalIncome - totalExpense,
                ExpenseCount = paycheck.Expenses.Count,
                IsClosed = paycheck.IsClosed
            };


        }

        public async Task<PaycheckResponse> UpdatePaycheckAsync(int id, UpdatePaycheckRequest request)
        {
            var paycheck = await _context.Paychecks.FirstOrDefaultAsync(x => x.Id == id && x.UserId == _currentUserService.UserId);
            if (paycheck == null)
                throw new NotFoundException("Paycheck not found.");
            if (paycheck.IsClosed)
                throw new BusinessRuleException("Cannot modify a closed paycheck.");

            paycheck.Description = request.Description;
            paycheck.ReceivedDate = request.ReceivedDate;
            await _context.SaveChangesAsync();
            _logger.LogInformation("Paycheck {PaycheckId} updated.", paycheck.Id);

            var totalIncome = await _context.Incomes.Where(i => i.PaycheckId == id).SumAsync(i => i.Amount);
            return new PaycheckResponse
            {
                Id = paycheck.Id,
                Description = paycheck.Description,
                TotalIncome = totalIncome,
                ReceivedDate = paycheck.ReceivedDate,
                IsClosed = paycheck.IsClosed
            };
        }

        public async Task DeletePaycheckAsync(int id)
        {
            var paycheck = await _context.Paychecks.FirstOrDefaultAsync(x => x.Id == id && x.UserId == _currentUserService.UserId);
            if (paycheck == null)
                throw new NotFoundException("Paycheck not found.");
            _context.Paychecks.Remove(paycheck);   // DB cascade removes its expenses
            await _context.SaveChangesAsync();
            _logger.LogInformation("Paycheck {PaycheckId} deleted.", id);
        }
    }
}