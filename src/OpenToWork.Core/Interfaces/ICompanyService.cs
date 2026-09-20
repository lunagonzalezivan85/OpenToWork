using OpenToWork.Shared.DTOs;

namespace OpenToWork.Core.Interfaces;

/// <summary>Servicio público de empresas (portal candidatos/empresas).</summary>
public interface ICompanyService
{
    /// <summary>Empresas visibles en el portal público, destacadas primero.</summary>
    Task<List<PublicCompanyDto>> GetPublicCompaniesAsync(int? limit = null);

    /// <summary>Detalle público de una empresa (perfil público del portal).</summary>
    Task<PublicCompanyDetailDto?> GetPublicCompanyAsync(Guid id);

    /// <summary>Perfil de la empresa del usuario autenticado.</summary>
    Task<MyCompanyProfileDto?> GetMyCompanyAsync(Guid userId);

    /// <summary>Actualiza el perfil de la empresa del usuario autenticado.</summary>
    Task<MyCompanyProfileDto?> UpdateMyCompanyAsync(Guid userId, UpdateMyCompanyProfileDto dto);
}
