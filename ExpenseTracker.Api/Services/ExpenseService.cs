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
            if (paycheck == null || paycheck.IsClosed)
            {
                return null; // Paycheck not found or is closed, cannot add expense
            }
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
            if (paycheck == null || paycheck.IsClosed)
            {
                return null; // Paycheck not found or is closed, cannot edit expense
            }
            var expense = await _context.Expenses.FindAsync(expenseId);
            if (expense == null || expense.PaycheckId != paycheckId)
            {
                return null; // Expense not found or does not belong to the specified paycheck
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

        public async Task<List<ExpenseResponse>> GetAllExpenses(int paycheckId)
        {
            var expenses = await _context.Expenses
                .Where(x => x.PaycheckId == paycheckId
                && x.Paycheck.UserId == _currentUserService.UserId)
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
            return expenses;
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
