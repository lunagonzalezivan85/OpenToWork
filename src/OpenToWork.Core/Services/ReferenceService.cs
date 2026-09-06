using Microsoft.EntityFrameworkCore;
using OpenToWork.Core.Interfaces;
using OpenToWork.Models.Context;
using OpenToWork.Models.Entities;
using OpenToWork.Shared.DTOs;
using OpenToWork.Shared.Enums;

namespace OpenToWork.Core.Services;

/// <summary>
/// Ver plan obligatorio de Fase 3 en README, sub-fase 3.5, y las decisiones documentadas en
/// docs/dsiezar/fase-3-sub5.md (por que no hay SMTP, umbral de X dias, etc).
/// </summary>
public class ReferenceService : IReferenceService
{
    private readonly AppDbContext _context;
    private readonly ITokenCryptoService _tokenCrypto;

    // Sin respuesta en 7 dias -> Failed, evaluado de forma perezosa (sin job programado, mismo
    // criterio ya usado en 3.2/3.3/3.4). El mismo plazo se usa como vencimiento del link.
    private const int ResponseTimeoutDays = 7;
    private const int MinimumReferencesRecommended = 3; // fase-3-sub5.md pregunta 1, informativo.

    public ReferenceService(AppDbContext context, ITokenCryptoService tokenCrypto)
    {
        _context = context;
        _tokenCrypto = tokenCrypto;
    }

    public async Task<CandidateReferenceDto> AddReferenceAsync(Guid candidateId, CreateReferenceDto dto)
    {
        var candidateExists = await _context.PT_Candidates.AnyAsync(c => c.Id == candidateId && !c.IsDeleted);
        if (!candidateExists) throw new InvalidOperationException("Candidate not found");

        var reference = new PTCandidateReference
        {
            PT_CandidateId = candidateId,
            ContactName = dto.ContactName,
            CompanyName = dto.CompanyName,
            Phone = dto.Phone,
            Email = dto.Email,
            Relationship = dto.Relationship,
            Status = (int)ReferenceStatus.Pending
        };

        _context.PT_CandidateReferences.Add(reference);
        await _context.SaveChangesAsync();

        var sameCompany = await HasSameCompanyAsAnotherAsync(candidateId, dto.CompanyName, reference.Id);
        return ToDto(reference, sameCompany);
    }

    public async Task<ReferenceRequestLinkDto?> SendReferenceRequestAsync(Guid candidateId, Guid referenceId)
    {
        var reference = await _context.PT_CandidateReferences
            .FirstOrDefaultAsync(r => r.Id == referenceId && r.PT_CandidateId == candidateId && !r.IsDeleted);
        if (reference == null) return null;

        // Sin SMTP en el proyecto (mismo gap ya documentado en AuthService.RequestPasswordResetAsync)
        // - se genera un token+link para que el candidato lo comparta manualmente (pregunta 2/3).
        var token = _tokenCrypto.GenerateRefreshToken();
        reference.TokenHash = _tokenCrypto.HashToken(token);
        reference.SentAt = DateTime.UtcNow;
        reference.TokenExpiresAt = DateTime.UtcNow.AddDays(ResponseTimeoutDays);
        reference.Status = (int)ReferenceStatus.Sent;
        reference.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new ReferenceRequestLinkDto
        {
            ReferenceId = reference.Id,
            ShareableLink = $"/references/respond?token={token}",
            ExpiresAt = reference.TokenExpiresAt.Value
        };
    }

    public async Task<bool> SubmitReferenceFeedbackAsync(string token, int rating, string? feedback)
    {
        var tokenHash = _tokenCrypto.HashToken(token);
        var reference = await _context.PT_CandidateReferences
            .FirstOrDefaultAsync(r => r.TokenHash == tokenHash && !r.IsDeleted);
        if (reference == null) return false;

        if (await ExpireIfStaleAsync(reference)) return false; // vencido - no se acepta la respuesta.
        if (reference.Status != (int)ReferenceStatus.Sent) return false; // ya respondida o invalida.

        reference.Rating = Math.Clamp(rating, 1, 5);
        reference.Feedback = feedback;
        reference.Status = (int)ReferenceStatus.Responded;
        reference.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // "El sistema valida la respuesta" (plan) - una vez que llega un rating en rango, se
        // verifica automaticamente, sin paso manual adicional (pregunta 4).
        await VerifyReferenceAsync(reference.Id);
        return true;
    }

    public async Task<CandidateReferenceDto?> VerifyReferenceAsync(Guid referenceId)
    {
        var reference = await _context.PT_CandidateReferences
            .FirstOrDefaultAsync(r => r.Id == referenceId && !r.IsDeleted);
        if (reference == null) return null;

        if (await ExpireIfStaleAsync(reference))
            return ToDto(reference, await HasSameCompanyAsAnotherAsync(reference.PT_CandidateId, reference.CompanyName, reference.Id));

        if (reference.Status == (int)ReferenceStatus.Responded && reference.Rating.HasValue)
        {
            reference.Status = (int)ReferenceStatus.Verified;
            reference.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Suma al EvidenceIndex (fase-3-sub5.md pregunta 5) - misma tabla/patron que
            // ValidationService, con VerificationType.Reference ya reservado desde 3.1.
            await UpsertReferenceVerificationAsync(reference.PT_CandidateId);
        }

        return ToDto(reference, await HasSameCompanyAsAnotherAsync(reference.PT_CandidateId, reference.CompanyName, reference.Id));
    }

    public async Task<CandidateReferencesListDto> GetReferencesAsync(Guid candidateId)
    {
        var references = await _context.PT_CandidateReferences
            .Where(r => r.PT_CandidateId == candidateId && !r.IsDeleted)
            .OrderBy(r => r.CreatedAt)
            .ToListAsync();

        var anyExpired = false;
        foreach (var r in references)
            anyExpired |= await ExpireIfStaleAsync(r);
        if (anyExpired) await _context.SaveChangesAsync();

        var companyCounts = references
            .Where(r => !string.IsNullOrWhiteSpace(r.CompanyName))
            .GroupBy(r => r.CompanyName!.Trim().ToLowerInvariant())
            .ToDictionary(g => g.Key, g => g.Count());

        return new CandidateReferencesListDto
        {
            References = references.Select(r => ToDto(r, IsDuplicateCompany(r, companyCounts))).ToList(),
            HasMinimumReferences = references.Count(r => !r.IsDeleted) >= MinimumReferencesRecommended
        };
    }

    /// <summary>
    /// Marca Failed si esta Sent y paso el plazo sin respuesta (pregunta 6). No guarda por si
    /// solo - el llamador decide cuando hacer SaveChangesAsync segun el flujo.
    /// </summary>
    private async Task<bool> ExpireIfStaleAsync(PTCandidateReference reference)
    {
        if (reference.Status != (int)ReferenceStatus.Sent || !reference.TokenExpiresAt.HasValue)
            return false;

        if (reference.TokenExpiresAt.Value >= DateTime.UtcNow)
            return false;

        reference.Status = (int)ReferenceStatus.Failed;
        reference.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    private async Task<bool> HasSameCompanyAsAnotherAsync(Guid candidateId, string? companyName, Guid excludeReferenceId)
    {
        if (string.IsNullOrWhiteSpace(companyName)) return false;

        var normalized = companyName.Trim().ToLowerInvariant();
        return await _context.PT_CandidateReferences
            .Where(r => r.PT_CandidateId == candidateId && !r.IsDeleted && r.Id != excludeReferenceId && r.CompanyName != null)
            .AnyAsync(r => r.CompanyName!.ToLower() == normalized);
    }

    private static bool IsDuplicateCompany(PTCandidateReference reference, Dictionary<string, int> companyCounts)
    {
        if (string.IsNullOrWhiteSpace(reference.CompanyName)) return false;
        var key = reference.CompanyName.Trim().ToLowerInvariant();
        return companyCounts.TryGetValue(key, out var count) && count > 1;
    }

    private async Task UpsertReferenceVerificationAsync(Guid candidateId)
    {
        var hasVerifiedReference = await _context.PT_CandidateReferences
            .AnyAsync(r => r.PT_CandidateId == candidateId && !r.IsDeleted && r.Status == (int)ReferenceStatus.Verified);

        var verification = await _context.PT_Verifications
            .FirstOrDefaultAsync(v => v.PT_CandidateId == candidateId && v.Type == (int)VerificationType.Reference && !v.IsDeleted);

        if (verification == null)
        {
            verification = new PTVerification { PT_CandidateId = candidateId, Type = (int)VerificationType.Reference };
            _context.PT_Verifications.Add(verification);
        }

        verification.Status = hasVerifiedReference ? (int)VerificationCheckStatus.Verified : (int)VerificationCheckStatus.Failed;
        verification.Score = hasVerifiedReference ? 100 : 0;
        verification.VerifiedAt = DateTime.UtcNow;
        verification.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    private static CandidateReferenceDto ToDto(PTCandidateReference r, bool sameCompanyAsAnother) => new()
    {
        Id = r.Id,
        ContactName = r.ContactName,
        CompanyName = r.CompanyName,
        Phone = r.Phone,
        Email = r.Email,
        Relationship = r.Relationship,
        Status = r.Status,
        Rating = r.Rating,
        Feedback = r.Feedback,
        SentAt = r.SentAt,
        SameCompanyAsAnotherReference = sameCompanyAsAnother
    };
}
