using Microsoft.AspNetCore.Mvc;
using OpenToWork.AdminAPI.Authorization;
using OpenToWork.Core.Interfaces;
using OpenToWork.Shared.Challenges;
using OpenToWork.Shared.Enums;

namespace OpenToWork.AdminAPI.Controllers;

/// <summary>
/// "Retos y competencias" (docs/dsiezar/retos-hosteleria.md). Permisos por accion:
/// editar = SuperAdmin + Reclutador; publicar/archivar/restaurar/carga inicial = solo SuperAdmin;
/// revisar = SuperAdmin (todo) o Reclutador (solo candidatos asignados, comprobado en el servicio);
/// ver resultados de un candidato = todo el equipo.
/// [RequireStaffRole] sin argumentos = solo SuperAdmin.
/// </summary>
[Route("api/admin/challenges")]
public class ChallengesController : AdminControllerBase
{
    private readonly IChallengeAdminService _admin;
    private readonly IChallengeReviewService _reviews;
    private readonly IChallengeService _challenges;

    public ChallengesController(IChallengeAdminService admin, IChallengeReviewService reviews, IChallengeService challenges)
    {
        _admin = admin;
        _reviews = reviews;
        _challenges = challenges;
    }

    // Igual que RequireStaffRole: sin claim cuenta como SuperAdmin (0).
    private bool IsSuperAdmin => (StaffRole ?? 0) == (int)AdminStaffRole.SuperAdmin;

    private IActionResult Action(ChallengeActionResultDto r) => r.Success ? Ok(r) : BadRequest(r);

    private IActionResult Map<T>(ChallengeOpResult<T> r) =>
        r.Error == null ? Ok(r.Value) : StatusCode(r.StatusCode, new { error = r.Error });

    // ---------------- Competencias ----------------

    [HttpGet("competencies")]
    [RequireStaffRole(AdminStaffRole.Reclutador)]
    public async Task<IActionResult> Competencies() => Ok(await _admin.GetCompetenciesAsync());

    [HttpPost("competencies")]
    [RequireStaffRole(AdminStaffRole.Reclutador)]
    public async Task<IActionResult> CreateCompetency([FromBody] SaveCompetencyDto dto) => Action(await _admin.CreateCompetencyAsync(dto, AdminId));

    [HttpPut("competencies/{id:guid}")]
    [RequireStaffRole(AdminStaffRole.Reclutador)]
    public async Task<IActionResult> UpdateCompetency(Guid id, [FromBody] SaveCompetencyDto dto) => Action(await _admin.UpdateCompetencyAsync(id, dto, AdminId));

    // ---------------- Cargos ----------------

    [HttpGet("job-types")]
    [RequireStaffRole(AdminStaffRole.Reclutador)]
    public async Task<IActionResult> JobTypes() => Ok(await _admin.GetJobTypesAsync());

    [HttpPut("job-types/{id:guid}")]
    [RequireStaffRole(AdminStaffRole.Reclutador)]
    public async Task<IActionResult> SaveJobType(Guid id, [FromBody] SaveChallengeJobTypeDto dto) => Action(await _admin.SaveJobTypeAsync(id, dto, AdminId));

    // ---------------- Retos ----------------

    [HttpGet]
    [RequireStaffRole(AdminStaffRole.Reclutador)]
    public async Task<IActionResult> List() => Ok(await _admin.GetChallengesAsync());

    [HttpGet("{id:guid}")]
    [RequireStaffRole(AdminStaffRole.Reclutador)]
    public async Task<IActionResult> Get(Guid id)
    {
        var c = await _admin.GetChallengeAsync(id);
        return c == null ? NotFound() : Ok(c);
    }

    [HttpPost]
    [RequireStaffRole(AdminStaffRole.Reclutador)]
    public async Task<IActionResult> Create([FromBody] SaveChallengeDto dto) => Action(await _admin.CreateChallengeAsync(dto, AdminId));

    [HttpPut("{id:guid}")]
    [RequireStaffRole(AdminStaffRole.Reclutador)]
    public async Task<IActionResult> SaveDraft(Guid id, [FromBody] SaveChallengeDto dto) => Action(await _admin.SaveDraftAsync(id, dto, AdminId));

    [HttpPost("{id:guid}/duplicate")]
    [RequireStaffRole(AdminStaffRole.Reclutador)]
    public async Task<IActionResult> Duplicate(Guid id) => Action(await _admin.DuplicateAsync(id, AdminId));

    [HttpDelete("{id:guid}")]
    [RequireStaffRole(AdminStaffRole.Reclutador)]
    public async Task<IActionResult> Delete(Guid id) => Action(await _admin.DeleteAsync(id, AdminId));

    [HttpPost("{id:guid}/publish")]
    [RequireStaffRole]
    public async Task<IActionResult> Publish(Guid id) => Action(await _admin.PublishAsync(id, AdminId));

    [HttpPost("{id:guid}/archive")]
    [RequireStaffRole]
    public async Task<IActionResult> Archive(Guid id) => Action(await _admin.ArchiveAsync(id, AdminId));

    [HttpPost("{id:guid}/restore")]
    [RequireStaffRole]
    public async Task<IActionResult> Restore(Guid id) => Action(await _admin.RestoreAsync(id, AdminId));

    [HttpPost("preview")]
    [RequireStaffRole(AdminStaffRole.Reclutador)]
    public IActionResult Preview([FromBody] PreviewEvaluateDto dto) => Ok(_admin.PreviewEvaluate(dto));

    [HttpPost("seed")]
    [RequireStaffRole]
    public async Task<IActionResult> Seed() => Ok(await _admin.SeedInitialContentAsync(AdminId));

    // ---------------- Revisiones ----------------

    [HttpGet("reviews")]
    [RequireStaffRole(AdminStaffRole.Reclutador)]
    public async Task<IActionResult> ReviewQueue([FromQuery] bool includeCompleted = false) =>
        Ok(await _reviews.GetQueueAsync(AdminId, IsSuperAdmin, includeCompleted));

    [HttpGet("reviews/{attemptId:guid}")]
    [RequireStaffRole(AdminStaffRole.Reclutador)]
    public async Task<IActionResult> ReviewDetail(Guid attemptId) => Map(await _reviews.GetDetailAsync(AdminId, IsSuperAdmin, attemptId));

    [HttpPut("reviews/{attemptId:guid}")]
    [RequireStaffRole(AdminStaffRole.Reclutador)]
    public async Task<IActionResult> SaveReview(Guid attemptId, [FromBody] SaveReviewDto dto) =>
        Map(await _reviews.SaveAsync(AdminId, IsSuperAdmin, attemptId, dto));

    // ---------------- Resultados de un candidato ----------------

    [HttpGet("candidates/{candidateUserId:guid}/results")]
    [RequireStaffRole(AdminStaffRole.Reclutador, AdminStaffRole.Comercial)]
    public async Task<IActionResult> CandidateResults(Guid candidateUserId) => Ok(await _challenges.GetResultsForAdminAsync(candidateUserId));
}
