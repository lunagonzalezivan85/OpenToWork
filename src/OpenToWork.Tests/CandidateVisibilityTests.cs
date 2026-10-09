using OpenToWork.Models.Entities;

namespace OpenToWork.Tests;

/// <summary>Visibilidad ante empresas (RGPD): IsProfilePublic vale true por defecto, pero sin
/// consentimiento registrado el candidato no es visible ni la casilla sale marcada.</summary>
public class CandidateVisibilityTests
{
    private static readonly DateTime T0 = new(2026, 10, 9, 10, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void PorDefecto_NoEsVisible_AunqueIsProfilePublicSeaTrue()
    {
        var c = new PTCandidate();
        Assert.True(c.IsProfilePublic);
        Assert.False(c.IsVisibleToCompanies);
    }

    [Fact]
    public void ConConsentimiento_EsVisible() =>
        Assert.True(new PTCandidate { VisibilityConsentAt = T0 }.IsVisibleToCompanies);

    [Fact]
    public void ConsentimientoRetirado_NoEsVisible() =>
        Assert.False(new PTCandidate { VisibilityConsentAt = T0, VisibilityConsentRevokedAt = T0.AddDays(1) }.IsVisibleToCompanies);

    [Fact]
    public void VueltoAAceptarTrasRetirar_EsVisible() =>
        Assert.True(new PTCandidate { VisibilityConsentAt = T0.AddDays(2), VisibilityConsentRevokedAt = T0.AddDays(1) }.IsVisibleToCompanies);

    [Fact]
    public void PerfilMarcadoComoPrivado_NoEsVisible() =>
        Assert.False(new PTCandidate { IsProfilePublic = false, VisibilityConsentAt = T0 }.IsVisibleToCompanies);
}
