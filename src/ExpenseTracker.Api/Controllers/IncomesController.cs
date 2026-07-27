using ExpenseTracker.Api.Dtos;
using ExpenseTracker.Api.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IncomesController : ControllerBase
    {
        private readonly IIncomeService _incomeService;

        public IncomesController(IIncomeService incomeService)
        {
            _incomeService = incomeService;
        }
        [ServiceFilter(typeof(ValidationFilter<CreateIncomeRequest>))]
        [HttpPost("{paycheckId}")]
        public async Task<IActionResult> CreateIncome(int paycheckId, CreateIncomeRequest request)
        {
            var income = await _incomeService.CreateIncomeAsync(paycheckId, request);
            return Ok(new ApiResponse<IncomeResponse>
            {
                success = true,
                message = "Income added successfully.",
                data = income
            });
        }

        [HttpGet("{paycheckId}")]
        public async Task<IActionResult> GetIncomes(int paycheckId)
        {
            var incomes = await _incomeService.GetIncomesAsync(paycheckId);
            return Ok(new ApiResponse<List<IncomeResponse>>
            {
                success = true,
                message = "Incomes retrieved successfully.",
                data = incomes
            });
        }

        [HttpDelete("{paycheckId}/{incomeId}")]
        public async Task<IActionResult> DeleteIncome(int paycheckId, int incomeId)
        {
            var result = await _incomeService.RemoveIncomeAsync(paycheckId, incomeId);
            if (!result)
            {
                return NotFound(new ApiResponse<string>
                {
                    success = false,
                    message = "Income not found or could not be deleted."
                });
            }
            return Ok(new ApiResponse<string>
            {
                success = true,
                message = "Income deleted successfully."
            });
        }
    }
}
