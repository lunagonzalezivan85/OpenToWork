using Microsoft.AspNetCore.Mvc;
using OpenToWork.AdminAPI.Authorization;
using OpenToWork.Core.Interfaces;
using OpenToWork.Shared.DTOs;
using OpenToWork.Shared.Enums;

namespace OpenToWork.AdminAPI.Controllers;

[Route("api/admin/vacancies/{vacancyId:guid}/contract")]
[RequireStaffRole(AdminStaffRole.Comercial)]
public class VacancyContractController : AdminControllerBase
{
    private readonly IAdminContractService _contractService;

    public VacancyContractController(IAdminContractService contractService)
    {
        _contractService = contractService;
    }

    [HttpGet]
    public async Task<IActionResult> GetContract(Guid vacancyId)
    {
        var contract = await _contractService.GetByVacancyAsync(vacancyId);
        return contract == null ? NotFound() : Ok(contract);
    }

    /// <summary>Crea (si no existe) o actualiza el anexo. Solo permitido en estado Draft.</summary>
    [HttpPut]
    public async Task<IActionResult> SaveContract(Guid vacancyId, [FromBody] AdminSaveVacancyContractDto dto)
    {
        var result = await _contractService.SaveAsync(vacancyId, dto, AdminId, ClientIp);
        return result == null ? BadRequest() : Ok(result);
    }

    /// <summary>Marca el anexo como enviado a la empresa (Draft -> Sent).</summary>
    [HttpPost("send")]
    public async Task<IActionResult> Send(Guid vacancyId)
    {
        var ok = await _contractService.SendAsync(vacancyId, AdminId, ClientIp);
        return ok ? NoContent() : BadRequest();
    }

    /// <summary>Registra la respuesta de la empresa: aceptacion o rechazo (con motivo).</summary>
    [HttpPost("decision")]
    public async Task<IActionResult> Decide(Guid vacancyId, [FromBody] AdminContractDecisionDto dto)
    {
        var ok = await _contractService.DecideAsync(vacancyId, dto.Accepted, dto.Reason, AdminId, ClientIp);
        return ok ? NoContent() : BadRequest();
    }
}
