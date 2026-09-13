using Microsoft.AspNetCore.Mvc;
using OpenToWork.AdminAPI.Authorization;
using OpenToWork.Core.Interfaces;
using OpenToWork.Shared.DTOs;
using OpenToWork.Shared.Enums;

namespace OpenToWork.AdminAPI.Controllers;

[Route("api/admin/contracts")]
[RequireStaffRole(AdminStaffRole.Comercial)]
public class VacancyContractController : AdminControllerBase
{
    private readonly IAdminContractService _contractService;
    private readonly IPromoCodeService _promoCodeService;

    public VacancyContractController(IAdminContractService contractService, IPromoCodeService promoCodeService)
    {
        _contractService = contractService;
        _promoCodeService = promoCodeService;
    }

    [HttpGet("{contractId:guid}")]
    public async Task<IActionResult> GetContract(Guid contractId)
    {
        var contract = await _contractService.GetByIdAsync(contractId);
        return contract == null ? NotFound() : Ok(contract);
    }

    [HttpGet("by-company/{companyId:guid}")]
    public async Task<IActionResult> GetByCompany(Guid companyId)
    {
        var contract = await _contractService.GetByCompanyAsync(companyId);
        return contract == null ? NotFound() : Ok(contract);
    }

    /// <summary>Crea un nuevo contrato para una empresa con N vacantes.</summary>
    [HttpPost("by-company/{companyId:guid}")]
    public async Task<IActionResult> CreateContract(Guid companyId, [FromBody] AdminSaveVacancyContractDto dto)
    {
        try
        {
            var result = await _contractService.CreateAsync(companyId, dto, AdminId, ClientIp);
            return result == null ? NotFound(new { error = "La empresa no existe." }) : Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Actualiza un contrato existente. Solo permitido en estado Draft.</summary>
    [HttpPut("{contractId:guid}")]
    public async Task<IActionResult> SaveContract(Guid contractId, [FromBody] AdminSaveVacancyContractDto dto)
    {
        try
        {
            var result = await _contractService.SaveAsync(contractId, dto, AdminId, ClientIp);
            return result == null ? NotFound(new { error = "El contrato no existe." }) : Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Valida un codigo promocional contra una vacante concreta (preview, sin guardar).</summary>
    [HttpPost("validate-promo")]
    public async Task<IActionResult> ValidatePromo([FromBody] ValidatePromoCodeDto dto)
    {
        var result = await _promoCodeService.ValidateAsync(dto.Code, dto.VacancyId);
        return Ok(result);
    }

    /// <summary>Marca el contrato como enviado a la empresa (Draft -> Sent).</summary>
    [HttpPost("{contractId:guid}/send")]
    public async Task<IActionResult> Send(Guid contractId)
    {
        var ok = await _contractService.SendAsync(contractId, AdminId, ClientIp);
        return ok ? NoContent() : BadRequest();
    }

    /// <summary>Registra la respuesta de la empresa: aceptacion o rechazo (con motivo).</summary>
    [HttpPost("{contractId:guid}/decision")]
    public async Task<IActionResult> Decide(Guid contractId, [FromBody] AdminContractDecisionDto dto)
    {
        var ok = await _contractService.DecideAsync(contractId, dto.Accepted, dto.Reason, AdminId, ClientIp);
        return ok ? NoContent() : BadRequest();
    }
}
