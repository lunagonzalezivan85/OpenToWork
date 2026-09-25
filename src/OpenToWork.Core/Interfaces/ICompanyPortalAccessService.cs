using OpenToWork.Shared.DTOs;

namespace OpenToWork.Core.Interfaces;

/// <summary>Acceso al portal de una empresa creada desde el admin (CRM/alta), que nace sin usuario.</summary>
public interface ICompanyPortalAccessService
{
    Task<CompanyPortalAccessDto?> GetStatusAsync(Guid companyId);

    /// <summary>Crea el usuario de la empresa con su correo de contacto (o renueva la invitacion si
    /// todavia no eligio contraseña) y devuelve el enlace para activarlo.</summary>
    Task<CompanyPortalInviteResultDto?> InviteAsync(Guid companyId, Guid adminId, string? ipAddress);
}
