using ExpenseTracker.Api.Dtos;
using ExpenseTracker.Api.Interface;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaychecksController : ControllerBase
    {
        private readonly IPaycheckService _paycheckService;

        public PaychecksController(IPaycheckService paycheckService, IValidator<CreateExpenseRequest> createExpenseValidator)
        {
            _paycheckService = paycheckService;

        }

        [Authorize]
        [HttpPost("CreatePaycheck")]
        [ServiceFilter(typeof(ValidationFilter<CreatePaycheckRequest>))]
        public async Task<IActionResult> CreatePaycheck(CreatePaycheckRequest createPaycheckRequest)
        {

            var paycheck = await _paycheckService.CreatePaycheckAsync(createPaycheckRequest);
            return Ok(new ApiResponse<PaycheckResponse>
            {
                success = true,
                message = "Paycheck created successfully",
                data = paycheck
            });
        }
        //[Authorize]
        [HttpGet("GetPaycheck")]
        public async Task<IActionResult> GetPaycheck(int id)
        {
            var paycheck = await _paycheckService.GetPaycheckAsync(id);
            if (paycheck == null) return NotFound(new ApiResponse<PaycheckResponse>
            {
                success = false,
                message = "Paycheck not found",
                data = null
            });
            return Ok(new ApiResponse<PaycheckResponse>
            {
                success = true,
                message = "Paycheck retrieved successfully",
                data = paycheck
            });
        }
        [HttpGet("GetPaychecks")]
        [ServiceFilter(typeof(ValidationFilter<PaycheckQueryRequest>))]
        public async Task<IActionResult> GetPaychecks([FromQuery] PaycheckQueryRequest request)
        {
            var paychecks = await _paycheckService.GetPaychecksAsync(request);
            return Ok(new ApiResponse<PagedResponse<PaycheckResponse>>
            {
                success = true,
                message = "Paychecks retrieved successfully",
                data = paychecks
            });
        }
        [HttpPost("ClosePaycheck")]
        public async Task<IActionResult> ClosePaycheck(int paycheckId, bool toggle)
        {
            var result = await _paycheckService.ClosePaycheck(paycheckId, toggle);
            if (!result) return NotFound(new ApiResponse<string>
            {
                success = false,
                message = "Paycheck not found",
                data = null
            });
            return Ok(new ApiResponse<string>
            {
                success = true,
                message = "Paycheck status changed successfully",
                data = null
            });
        }

        [HttpGet("GetSummary")]
        public async Task<IActionResult> GetSummary(int paycheckId)
        {
            var summary = await _paycheckService.GetSummaryAsync(paycheckId);
            if (summary == null) return NotFound(new ApiResponse<PaycheckSummaryResponse>
            {
                success = false,        
                message = "Paycheck not found",
                data = null
            });
            return Ok(new ApiResponse<PaycheckSummaryResponse>
            {   
                success = true,
                message = "Paycheck summary retrieved successfully",
                data = summary
            });
        }

        [HttpPut("UpdatePaycheck")]
        public async Task<IActionResult> UpdatePaycheck(int id, UpdatePaycheckRequest request)
        {
            var updated = await _paycheckService.UpdatePaycheckAsync(id, request);
            return Ok(new ApiResponse<PaycheckResponse>
            {
                success = true,
                message = "Paycheck updated successfully",
                data = updated
            });

        }
        [HttpDelete("DeletePaycheck")]
        public async Task<IActionResult> DeletePaycheck(int id)
        {
            await _paycheckService.DeletePaycheckAsync(id);
            return Ok(new ApiResponse<string> { success = true, message = "Paycheck deleted successfully" });
        }

    }
}
