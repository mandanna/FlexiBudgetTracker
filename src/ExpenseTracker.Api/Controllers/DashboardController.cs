using ExpenseTracker.Api.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {

        public DashboardController() { }

        //public async Task<IActionResult> GetDashboardData()
        //{
        //    // Implement logic to retrieve dashboard data
        //    var dashboardData = new
        //    {
        //        TotalExpenses = 1000,
        //        TotalIncome = 2000,
        //        NetBalance = 1000
        //    };
        //    return Ok(new ApiResponse<object>
        //    {
        //        success = true,
        //        message = "Dashboard data retrieved successfully",
        //        data = dashboardData
        //    });
        //}
    }
}
