using ExpenseTracker.Api.Data;
using ExpenseTracker.Api.Dtos;
using ExpenseTracker.Api.Exceptions;
using ExpenseTracker.Api.Interface;
using ExpenseTracker.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Api.Services
{
    public class ExpenseService : IExpenseService
    {
        private readonly ExpenseTrackerDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<ExpenseService> _logger;
        public ExpenseService(ExpenseTrackerDbContext context, ICurrentUserService currentUserService, ILogger<ExpenseService> logger)
        {
            _context = context;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<ExpenseResponse?> CreateExpenseAsync(int paycheckId, CreateExpenseRequest createExpenseRequest)
        {
            var paycheck = await _context.Paychecks.FirstOrDefaultAsync(x=>x.Id==paycheckId && x.UserId==_currentUserService.UserId);
            if (paycheck == null)
                throw new NotFoundException($"Paycheck with ID {paycheckId} not found for the current user.");
            if (paycheck.IsClosed)
                throw new BusinessRuleException("Cannot add expense to a closed paycheck.");

            var existingExpensesTotal = await _context.Expenses.Where(x => x.PaycheckId == paycheckId).SumAsync(x => x.Amount);
            if (createExpenseRequest.Amount > (paycheck.Amount - existingExpensesTotal))
            {
                throw new BusinessRuleException("Expense amount exceeds the remaining paycheck balance.");
            }
            var expense = new Expense
            {
                Amount = createExpenseRequest.Amount,
                Description = createExpenseRequest.Description,
                ExpenseDate = createExpenseRequest.ExpenseDate,
                PaycheckId = paycheckId,
                CategoryId = createExpenseRequest.CategoryId==0?null: createExpenseRequest.CategoryId
            };
            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Expense {Expense Id} created for paycheck {PaycheckId}",expense.Id,paycheckId);
            return new ExpenseResponse
            {
                Id = expense.Id,
                Amount = expense.Amount,
                Description = expense.Description,
                ExpenseDate = expense.ExpenseDate,
                PaycheckId = expense.PaycheckId
            };
        }

        public async Task<ExpenseResponse?> UpdateExpenseAsync(int paycheckId,int expenseId, UpdateExpenseRequest updateExpenseRequest) {

            var paycheck = await _context.Paychecks.FirstOrDefaultAsync(x => x.Id == paycheckId && x.UserId == _currentUserService.UserId);

            if (paycheck == null)
                throw new NotFoundException($"Paycheck with ID {paycheckId} not found for the current user.");
            if (paycheck.IsClosed)
                throw new BusinessRuleException("Cannot add expense to a closed paycheck.");
            var expense = await _context.Expenses.FindAsync(expenseId);

            if (expense == null || expense.PaycheckId != paycheckId)
                throw new NotFoundException("Expense not found.");

            var existingExpensesTotal = await _context.Expenses.Where(x => x.PaycheckId == paycheckId).SumAsync(x => x.Amount);
            if (updateExpenseRequest.Amount > (paycheck.Amount - existingExpensesTotal))
            {
                throw new BusinessRuleException("Expense amount exceeds the remaining paycheck balance.");
            }
            expense.Description = updateExpenseRequest.Description;
            expense.Amount = updateExpenseRequest.Amount;
            expense.ExpenseDate = updateExpenseRequest.ExpenseDate;
            expense.CategoryId = updateExpenseRequest.CategoryId;
            
            await _context.SaveChangesAsync();

            _logger.LogInformation("Expense {Expense Id} updated for paycheck {PaycheckId}", expense.Id, paycheckId);
            return new ExpenseResponse
            {
                Id = expense.Id,
                Amount = expense.Amount,
                Description = expense.Description,
                ExpenseDate = expense.ExpenseDate,
                PaycheckId = expense.PaycheckId
            };
        }

        public async Task<PagedResponse<ExpenseResponse>> GetAllExpenses(ExpenseQueryRequest request)
        {
            // The paycheck join is the authorization boundary: an expense is only ever reachable
            // through a paycheck the current user owns.
            IQueryable<Expense> query = _context.Expenses
                .Where(x => x.Paycheck.UserId == _currentUserService.UserId);

            if (request.PaycheckId.HasValue)
            {
                query = query.Where(x => x.PaycheckId == request.PaycheckId.Value);
            }

            if (request.CategoryId.HasValue)
            {
                query = query.Where(x => x.CategoryId == request.CategoryId.Value);
            }

            if (request.FromDate.HasValue)
            {
                query = query.Where(x => x.ExpenseDate >= request.FromDate.Value);
            }

            if (request.ToDate.HasValue)
            {
                query = query.Where(x => x.ExpenseDate <= request.ToDate.Value);
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
                .Select(x => new ExpenseResponse
                {
                    Id = x.Id,
                    Description = x.Description,
                    Amount = x.Amount,
                    ExpenseDate = x.ExpenseDate,
                    PaycheckId = x.PaycheckId,
                    CategoryId = x.CategoryId,
                    CategoryName = x.Category != null ? x.Category.Name : "Uncategorized"
                })
                .ToListAsync();

            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            _logger.LogInformation(
                "Retrieved {Count} expense(s) (page {Page} of {TotalPages}, {TotalRecords} total) for user {UserId}.",
                items.Count, page, totalPages, totalRecords, _currentUserService.UserId);

            return new PagedResponse<ExpenseResponse>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                TotalPages = totalPages
            };
        }

        private static IQueryable<Expense> ApplySort(IQueryable<Expense> query, string? sortBy, bool descending)
        {
            // A whitelist switch rather than reflection or dynamic LINQ: every branch is a
            // compile-time-checked property access, so no user-supplied string can reach an
            // unintended property. Unrecognised values are rejected by the validator before
            // they get here; the default branch orders by newest expense first.
            return sortBy?.Trim().ToLowerInvariant() switch
            {
                "amount" => descending ? query.OrderByDescending(x => x.Amount) : query.OrderBy(x => x.Amount),
                "description" => descending ? query.OrderByDescending(x => x.Description) : query.OrderBy(x => x.Description),
                "categoryname" => descending ? query.OrderByDescending(x => x.Category!.Name) : query.OrderBy(x => x.Category!.Name),
                "expensedate" => descending ? query.OrderByDescending(x => x.ExpenseDate) : query.OrderBy(x => x.ExpenseDate),
                _ => query.OrderByDescending(x => x.ExpenseDate)
            };
        }


        public async Task<ExpenseResponse> GetExpenseAsync(int paycheckId, int expenseId)
        {
            var expense=await _context.Expenses
                .Where(
                x=>x.PaycheckId==paycheckId &&
                x.Id==expenseId &&
                x.Paycheck.UserId==_currentUserService.UserId)

                .Select(x=>new ExpenseResponse {
                Id = x.Id,
                Description = x.Description,
                Amount = x.Amount,
                ExpenseDate = x.ExpenseDate,
                PaycheckId = x.PaycheckId
            })
                .FirstOrDefaultAsync();
            return expense;
        }

      
        public async Task<bool> RemoveExpenseAsync(int paycheckId,int expenseId)
        {
            var expense = await _context.Expenses
                .Where(x=>x.PaycheckId==paycheckId &&
                x.Paycheck.UserId==_currentUserService.UserId &&
                x.Id==expenseId)
                .FirstOrDefaultAsync();
            if (expense == null || expense.PaycheckId!=paycheckId)
            {
                return false;
            }
            _context.Expenses.Remove(expense);
            var changes = await _context.SaveChangesAsync();

            _logger.LogInformation("Expense {Expense Id} deleted for paycheck {PaycheckId}", expense.Id, paycheckId);
            return changes > 0;
        }
    }
}
