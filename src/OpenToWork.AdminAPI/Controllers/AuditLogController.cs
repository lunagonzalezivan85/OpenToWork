using Microsoft.AspNetCore.Mvc;
using OpenToWork.AdminAPI.Authorization;
using OpenToWork.Core.Interfaces;

namespace OpenToWork.AdminAPI.Controllers;

[Route("api/admin/audit-log")]
[RequireStaffRole]
public class AuditLogController : AdminControllerBase
{
    private readonly IAuditLogService _auditLogService;

    public AuditLogController(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    [HttpGet]
    public async Task<IActionResult> GetLogs([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var logs = await _auditLogService.GetLogsAsync(page, pageSize);
        return Ok(logs);
    }
}
