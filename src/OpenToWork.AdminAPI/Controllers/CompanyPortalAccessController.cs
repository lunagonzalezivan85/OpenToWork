using Microsoft.AspNetCore.Mvc;
using OpenToWork.AdminAPI.Authorization;
using OpenToWork.Core.Interfaces;
using OpenToWork.Shared.Enums;

namespace OpenToWork.AdminAPI.Controllers;

/// <summary>Acceso al portal de empresas creadas desde el admin (invitacion con enlace para elegir contraseña).</summary>
[Route("api/admin/company-crm/companies/{companyId:guid}/portal-access")]
[RequireStaffRole(AdminStaffRole.Comercial)]
public class CompanyPortalAccessController : AdminControllerBase
{
    private readonly ICompanyPortalAccessService _portalAccess;

    public CompanyPortalAccessController(ICompanyPortalAccessService portalAccess)
    {
        _portalAccess = portalAccess;
    }

    [HttpGet]
    public async Task<IActionResult> Status(Guid companyId)
    {
        var result = await _portalAccess.GetStatusAsync(companyId);
        return result == null ? NotFound() : Ok(result);
    }

    /// <summary>Crea el usuario de la empresa (o renueva la invitacion) y devuelve el enlace de activacion.</summary>
    [HttpPost("invite")]
    public async Task<IActionResult> Invite(Guid companyId)
    {
        try
        {
            var result = await _portalAccess.InviteAsync(companyId, AdminId, ClientIp);
            return result == null ? NotFound() : Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
