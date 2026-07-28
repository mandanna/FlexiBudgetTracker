namespace ExpenseTracker.Api.Dtos
{
    public class DashboardQueryRequest
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? PaycheckId { get; set; }
    }
}
