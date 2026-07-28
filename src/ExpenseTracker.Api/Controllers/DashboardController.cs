using Azure.Core;
using ExpenseTracker.Api.Dtos;
using ExpenseTracker.Api.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)=> _dashboardService = dashboardService;

        [HttpGet]
        [ServiceFilter(typeof(ValidationFilter<DashboardQueryRequest>))]
        public async Task<IActionResult> GetDashboardData([FromQuery] DashboardQueryRequest request)
        {

            var dashboardData = await _dashboardService.GetDashboardAsync(request);
            if (dashboardData == null)
            {
                return NotFound(new ApiResponse<DashboardResponse>
                {
                    success = false,
                    message = "Dashboard data not found",
                    data = null
                });
            }
            return Ok(new ApiResponse<DashboardResponse>
            {
                success = true,
                message = "Dashboard data retrieved successfully",
                data = dashboardData
            });

        }
    }
}
