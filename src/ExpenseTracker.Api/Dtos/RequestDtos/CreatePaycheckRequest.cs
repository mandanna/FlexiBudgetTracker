using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Api.Dtos
{
    public class CreatePaycheckRequest
    {
        public string Description { get; set; } = string.Empty;
        public DateTime ReceivedDate { get; set; }
        public List<CreateIncomeRequest> IncomesToCreate { get; set; } = new();
    }
}
