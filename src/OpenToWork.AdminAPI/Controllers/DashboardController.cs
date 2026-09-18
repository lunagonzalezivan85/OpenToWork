using Microsoft.AspNetCore.Mvc;
using OpenToWork.Core.Interfaces;

namespace OpenToWork.AdminAPI.Controllers;

[Route("api/admin/dashboard")]
public class DashboardController : AdminControllerBase
{
    private readonly IAdminDashboardService _dashboardService;

    public DashboardController(IAdminDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("metrics")]
    public async Task<IActionResult> GetMetrics()
    {
        var metrics = await _dashboardService.GetMetricsAsync();
        return Ok(metrics);
    }

    [HttpGet("business-metrics")]
    public async Task<IActionResult> GetBusinessMetrics()
    {
        var metrics = await _dashboardService.GetBusinessMetricsAsync();
        return Ok(metrics);
    }
}
