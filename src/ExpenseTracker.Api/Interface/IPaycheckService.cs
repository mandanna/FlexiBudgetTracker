using ExpenseTracker.Api.Dtos;
using ExpenseTracker.Api.Dtos.ResponseDtos;
namespace ExpenseTracker.Api.Interface
{
    public interface IPaycheckService
    {
        Task<PaycheckResponse> CreatePaycheckAsync(CreatePaycheckRequest request);
        Task<PaycheckResponse> UpdatePaycheckAsync(int id, UpdatePaycheckRequest request);
        Task<PaycheckResponse>GetPaycheckAsync(int id);
        Task<PagedResponse<PaycheckResponse>> GetPaychecksAsync(PaycheckQueryRequest request);
        Task<bool> ClosePaycheck(int paycheckId,bool toggle);
        Task<PaycheckSummaryResponse?> GetSummaryAsync(int paycheckId);
        //Task<List<PaycheckDetailsResponse>> GetDashboardAsync();

        Task DeletePaycheckAsync(int id);
    }
}
