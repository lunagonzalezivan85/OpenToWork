using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenToWork.Core.Interfaces;
using OpenToWork.Models.Context;
using OpenToWork.Shared.DTOs;

namespace OpenToWork.AdminAPI.Controllers;

/// <summary>
/// Cola de solicitudes "presentadme a este candidato" de empresas verificadas (reapertura
/// controlada de H-39). El staff las revisa y la entrega real se hace por el pipeline
/// normal (recruitment -> ReadyToDeliver -> DeliverCandidateAsync).
/// </summary>
[Route("api/admin/candidate-requests")]
public class CandidateRequestsController : AdminControllerBase
{
    private readonly AppDbContext _context;
    private readonly IAuditLogService _auditLog;

    public CandidateRequestsController(AppDbContext context, IAuditLogService auditLog)
    {
        _context = context;
        _auditLog = auditLog;
    }

    /// <summary>Solicitudes pendientes (status=0) primero; el resto al final.</summary>
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] bool all = false)
    {
        var query = _context.PT_CandidateRequests
            .Include(r => r.Company)
            .Include(r => r.Candidate)
            .Include(r => r.Vacancy)
            .Include(r => r.RequestedByUser)
            .Where(r => !r.IsDeleted);

        if (!all) query = query.Where(r => r.Status == 0);

        var items = await query
            .OrderBy(r => r.Status).ThenByDescending(r => r.CreatedAt)
            .Select(r => new CandidateRequestListDto
            {
                Id = r.Id,
                CompanyName = r.Company.Name,
                CandidateMaskedName = r.Candidate.FirstName + " " + r.Candidate.LastName.Substring(0, 1) + ".",
                CandidateTitle = r.Candidate.Title,
                CandidateId = r.PT_CandidateId,
                CandidateUserId = r.Candidate.SCUserId,
                CompanyId = r.PT_CompanyId,
                VacancyTitle = r.Vacancy != null ? r.Vacancy.Title : null,
                VacancyId = r.PT_VacancyId,
                Notes = r.Notes,
                Status = r.Status,
                RequestedByEmail = r.RequestedByUser.Email,
                CreatedAt = r.CreatedAt,
                ReviewedAt = r.ReviewedAt
            })
            .ToListAsync();

        return Ok(items);
    }

    /// <summary>Marcar como gestionada (el staff ya la vio / preparo la entrega por el pipeline).</summary>
    [HttpPost("{id}/handle")]
    public Task<IActionResult> Handle(Guid id) => ReviewAsync(id, 1, "handled");

    /// <summary>Rechazar la solicitud (no procede / empresa no encaja).</summary>
    [HttpPost("{id}/reject")]
    public Task<IActionResult> Reject(Guid id) => ReviewAsync(id, 2, "rejected");

    private async Task<IActionResult> ReviewAsync(Guid id, int newStatus, string action)
    {
        var request = await _context.PT_CandidateRequests
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
        if (request == null) return NotFound();
        if (request.Status != 0) return Conflict(new { error = "already_reviewed" });

        request.Status = newStatus;
        request.ReviewedByUserId = AdminId;
        request.ReviewedAt = DateTime.UtcNow;
        request.UpdatedAt = DateTime.UtcNow;
        request.UpdatedBy = AdminId;
        await _context.SaveChangesAsync();

        await _auditLog.LogAsync(AdminId, $"CandidateRequest.{action}", "PTCandidateRequest", id, null, ClientIp);
        return NoContent();
    }
}
