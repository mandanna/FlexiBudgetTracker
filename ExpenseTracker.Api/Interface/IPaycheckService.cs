using ExpenseTracker.Api.Dtos;
using ExpenseTracker.Api.Dtos.RequestDtos.QueryRequest.Paycheck;
using ExpenseTracker.Api.Dtos.ResponseDtos;
namespace ExpenseTracker.Api.Interface
{
    public interface IPaycheckService
    {
        Task<PaycheckResponse> CreatePaycheckAsync(CreatePaycheckRequest request);
        Task<PaycheckResponse>GetPaycheckAsync(int id);
        Task<List<PaycheckResponse>> GetPaychecksAsync();
        Task<bool> ClosePaycheck(int paycheckId,bool toggle);
        Task<PaycheckSummaryResponse?> GetSummaryAsync(int paycheckId);
        Task<List<PaycheckDetailsResponse>> GetDashboardAsync();
    }
}
