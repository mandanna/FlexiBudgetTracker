using ExpenseTracker.Api.Dtos;

namespace ExpenseTracker.Api.Interface
{
    public interface IExpenseService
    {
        Task<ExpenseResponse?> CreateExpenseAsync(int paycheckId, CreateExpenseRequest createExpenseRequest);
        Task<ExpenseResponse> GetExpenseAsync(int paycheckId, int expenseId);

        Task<PagedResponse<ExpenseResponse>> GetAllExpenses(ExpenseQueryRequest request);
        Task<bool> RemoveExpenseAsync(int paycheckId, int expenseId);
        Task<ExpenseResponse?> UpdateExpenseAsync(int paycheckId, int expenseId, UpdateExpenseRequest updateExpenseRequest);


    }
}
