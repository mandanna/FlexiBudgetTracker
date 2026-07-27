using ExpenseTracker.Api.Dtos;

namespace ExpenseTracker.Api.Interface
{
    public interface IIncomeService
    {
        Task<IncomeResponse> CreateIncomeAsync(int paycheckId, CreateIncomeRequest request);
        Task<List<IncomeResponse>> GetIncomesAsync(int paycheckId);
        Task<bool> RemoveIncomeAsync(int paycheckId, int incomeId);
    }
}
