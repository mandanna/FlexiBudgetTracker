using ExpenseTracker.Api.Data;
using ExpenseTracker.Api.Dtos;
using ExpenseTracker.Api.Exceptions;
using ExpenseTracker.Api.Interface;
using ExpenseTracker.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Api.Services
{
    public class IncomeService:IIncomeService
    {
        private readonly ExpenseTrackerDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<IncomeService> _logger;
        public IncomeService(ExpenseTrackerDbContext context, ICurrentUserService currentUserService, ILogger<IncomeService> logger)
        {
            _context = context;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<IncomeResponse> CreateIncomeAsync(int paycheckId, CreateIncomeRequest request)
        {
            var paycheck = await _context.Paychecks
                 .FirstOrDefaultAsync(x => x.Id == paycheckId && x.UserId == _currentUserService.UserId);
            if (paycheck == null)
                throw new NotFoundException($"Paycheck with ID {paycheckId} not found for the current user.");
            if (paycheck.IsClosed)
                throw new BusinessRuleException("Cannot add income to a closed paycheck.");
            var income = new Income
            {
                Source = request.Source,
                Amount = request.Amount,
                ReceivedDate = request.ReceivedDate ?? DateTime.Now,
                PaycheckId = paycheckId
            };
            _context.Incomes.Add(income);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Income {IncomeId} added to paycheck {PaycheckId}.", income.Id, paycheckId);
            return new IncomeResponse
            {
                Id = income.Id,
                Source = income.Source,
                Amount = income.Amount,
                ReceivedDate = income.ReceivedDate,
                PaycheckId = income.PaycheckId
            };
        }

        public async Task<List<IncomeResponse>> GetIncomesAsync(int paycheckId)
        {
            return await _context.Incomes
                 .Where(i => i.PaycheckId == paycheckId && i.Paycheck.UserId == _currentUserService.UserId)
                 .Select(i => new IncomeResponse
                 {
                     Id = i.Id,
                     Source = i.Source,
                     Amount = i.Amount,
                     ReceivedDate = i.ReceivedDate,
                     PaycheckId = i.PaycheckId
                 })
                 .ToListAsync();
        }

        public async Task<bool> RemoveIncomeAsync(int paycheckId, int incomeId)
        {
            var income = await _context.Incomes
                .FirstOrDefaultAsync(i => i.Id == incomeId
                    && i.PaycheckId == paycheckId
                    && i.Paycheck.UserId == _currentUserService.UserId);
            if (income == null)
                return false;

            _context.Incomes.Remove(income);
            var changes = await _context.SaveChangesAsync();

            _logger.LogInformation("Income {IncomeId} removed from paycheck {PaycheckId}.", incomeId, paycheckId);
            return changes > 0;
        }
    }
}
