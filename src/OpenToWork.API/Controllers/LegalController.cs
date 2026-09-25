using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenToWork.Core.Interfaces;
using OpenToWork.Shared.DTOs;

namespace OpenToWork.API.Controllers;

/// <summary>Datos publicos para las paginas legales del portal (politica de privacidad).</summary>
[ApiController]
[Route("api/legal")]
[AllowAnonymous]
public class LegalController : ControllerBase
{
    private readonly ISystemConfigService _systemConfig;

    public LegalController(ISystemConfigService systemConfig)
    {
        _systemConfig = systemConfig;
    }

    /// <summary>Responsable del tratamiento: razon social, CIF, domicilio, registro y correo de privacidad
    /// (editables en el admin, Datos de la Empresa). Sin el DNI del representante legal.</summary>
    [HttpGet("identity")]
    public async Task<IActionResult> Identity()
    {
        var identity = await _systemConfig.GetCompanyIdentityAsync();
        return Ok(new PublicLegalIdentityDto
        {
            LegalName = identity.LegalName,
            TaxId = identity.TaxId,
            Address = identity.Address,
            MercantileRegistry = identity.MercantileRegistry,
            PrivacyEmail = identity.PrivacyEmail
        });
    }
}
