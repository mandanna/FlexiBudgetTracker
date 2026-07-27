using ExpenseTracker.Api.Dtos;
using ExpenseTracker.Api.Interface;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExpensesController : ControllerBase
    {
        private readonly IExpenseService _expenseService;
        public ExpensesController(IExpenseService expenseService)
        {
            _expenseService = expenseService;
        }

        [ServiceFilter(typeof(ValidationFilter<CreateExpenseRequest>))]
        [HttpPost("{paycheckId}/Createxpenses")]
        public async Task<IActionResult> CreateExpense(int paycheckId, CreateExpenseRequest request)
        {
            //var validationRes = await _createExpenseValidator.ValidateAsync(request);
            //if (!validationRes.IsValid)
            //{
            //    return BadRequest(validationRes.Errors);
            //}
            var expense = await _expenseService
                .CreateExpenseAsync(paycheckId, request);

            if (expense == null)
            {
                return BadRequest(
                    new ApiResponse<string>
                    {
                        success = false,
                        message = "Unable to create expense."
                    });
            }
            return Ok(
                new ApiResponse<ExpenseResponse>
                {
                    success = true,
                    message = "Expense created successfully.",
                    data = expense
                });
        }
        [ServiceFilter(typeof(ValidationFilter<UpdateExpenseRequest>))]
        [HttpPut("{paycheckId}/expense/{expenseId}")]
        public async Task<IActionResult> UpdateExpense(int paycheckId, int expenseId, UpdateExpenseRequest request)
        {
            var expense = await _expenseService
                .UpdateExpenseAsync(paycheckId, expenseId, request);
            if (expense == null)
            {
                return NotFound(
                    new ApiResponse<string>
                    {
                        success = false,
                        message = "Expense not found."
                    });
            }
            return Ok(
                new ApiResponse<ExpenseResponse>
                {
                    success = true,
                    message = "Expense updated successfully.",
                    data = expense
                });
        }

        [HttpGet("{paycheckId}/expenses/{expenseId}")]
        public async Task<IActionResult> GetExpense(int paycheckId, int expenseId)
        {
            var expense = await _expenseService
                .GetExpenseAsync(paycheckId, expenseId);

            if (expense == null)
            {
                return NotFound();
            }

            return Ok(new ApiResponse<ExpenseResponse>
            {
                success = true,
                message = "Expense found",
                data = expense
            });
        }

        [HttpGet("expenses")]
        public async Task<IActionResult> GetAllExpenses([FromQuery] ExpenseQueryRequest expenseQueryRequest)
        {
            var expenses = await _expenseService.GetAllExpenses(expenseQueryRequest);
            return Ok(new ApiResponse<PagedResponse<ExpenseResponse>>
            {
                success = true,
                message = "Expenses retrieved successfully",
                data = expenses
            });
        }

        [HttpDelete("{paycheckId}/expenses/{expenseId}")]
        public async Task<IActionResult> DeleteExpense(int paycheckId, int expenseId)
        {
            var result = await _expenseService.RemoveExpenseAsync(paycheckId, expenseId);
            if (!result)
            {
                return NotFound(new ApiResponse<string>
                {
                    success = false,
                    message = "Expense not found or could not be deleted."
                });
            }
            return Ok(new ApiResponse<string>
            {
                success = true,
                message = "Expense deleted successfully."
            });
        }
    }
}
