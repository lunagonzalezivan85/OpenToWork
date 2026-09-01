using Microsoft.EntityFrameworkCore;
using OpenToWork.Core.Interfaces;
using OpenToWork.Models.Context;
using OpenToWork.Models.Entities;
using OpenToWork.Shared.DTOs;
using OpenToWork.Shared.Enums;

namespace OpenToWork.Core.Services;

/// <summary>
/// Ver plan obligatorio de Fase 3 en README, sub-fase 3.7, y las decisiones documentadas en
/// docs/dsiezar/fase-3-sub7.md (umbral de score, por que Identity queda afuera, interpretacion
/// de la secuencia de estados, etc).
/// </summary>
public class VerificationStatusService : IVerificationStatusService
{
    private readonly AppDbContext _context;

    private const int VerifiedTDScoreThreshold = 70; // pregunta 1.
    private const int ProfileCompleteThreshold = 80; // ya fijado por el plan (no es pregunta).
    private const int EvaluatedMinCompletedChecks = 3; // ya fijado por el plan (no es pregunta).

    // Los 4 checks que cuentan para la progresion - Identity queda afuera (pregunta 2): es un
    // stub permanente (VerifyIdentityAsync siempre Pending, fase-3-sub2.md), exigirlo haria
    // "Verificado TD" inalcanzable para siempre.
    private static readonly VerificationType[] GatingVerificationTypes =
    {
        VerificationType.LinkedIn, VerificationType.Portfolio, VerificationType.CvCoherence, VerificationType.Reference
    };

    // Version int - EF Core/LINQ-to-SQL no traduce bien un array.Contains sobre un enum
    // casteado dentro del predicado (ver fase-3-sub7.md, nota tecnica).
    private static readonly List<int> GatingVerificationTypeInts = GatingVerificationTypes.Select(t => (int)t).ToList();

    public VerificationStatusService(AppDbContext context)
    {
        _context = context;
    }

    public Task<VerificationStatusDto> GetVerificationStatusAsync(Guid candidateId) => EvaluateVerificationStatusAsync(candidateId);

    public async Task<VerificationStatusDto> EvaluateVerificationStatusAsync(Guid candidateId)
    {
        var candidate = await _context.PT_Candidates.FirstOrDefaultAsync(c => c.Id == candidateId && !c.IsDeleted);
        if (candidate == null) throw new InvalidOperationException("Candidate not found");

        var profileCompletion = CalculateProfileCompletion(candidate);

        var score = await _context.PT_CandidateScores
            .FirstOrDefaultAsync(s => s.PT_CandidateId == candidateId && !s.IsDeleted);
        var overallScore = score?.OverallScore ?? 0;

        var gatingVerifications = await _context.PT_Verifications
            .Where(v => v.PT_CandidateId == candidateId && !v.IsDeleted && GatingVerificationTypeInts.Contains(v.Type))
            .ToListAsync();
        var gatingRun = gatingVerifications.Count;
        var gatingVerified = gatingVerifications.Count(v => v.Status == (int)VerificationCheckStatus.Verified);

        var hasVerifiedReference = await _context.PT_CandidateReferences
            .AnyAsync(r => r.PT_CandidateId == candidateId && !r.IsDeleted && r.Status == (int)ReferenceStatus.Verified);

        var allGatingPassed = gatingRun == GatingVerificationTypes.Length && gatingVerified == GatingVerificationTypes.Length;

        // Interpretacion de la secuencia Perfil registrado -> ... -> Verificado TD (ver
        // fase-3-sub7.md, nota de interpretacion): exactamente 3 de 4 corridas = Evaluado; las
        // 4 corridas pero sin cumplir todo = Verificacion en proceso; todo cumplido = Verificado TD.
        CandidateVerificationStatus status;
        if (allGatingPassed && overallScore >= VerifiedTDScoreThreshold && hasVerifiedReference)
            status = CandidateVerificationStatus.VerifiedTD;
        else if (gatingRun == GatingVerificationTypes.Length)
            status = CandidateVerificationStatus.InProgress;
        else if (overallScore > 0 && gatingRun >= EvaluatedMinCompletedChecks)
            status = CandidateVerificationStatus.Evaluated;
        else if (gatingRun > 0)
            status = CandidateVerificationStatus.InProgress;
        else if (profileCompletion >= ProfileCompleteThreshold)
            status = CandidateVerificationStatus.ProfileComplete;
        else
            status = CandidateVerificationStatus.ProfileRegistered;

        return new VerificationStatusDto
        {
            CandidateId = candidateId,
            Status = (int)status,
            IsVerifiedTD = status == CandidateVerificationStatus.VerifiedTD,
            ProfileCompletionPercentage = profileCompletion,
            OverallScore = overallScore,
            GatingChecksRun = gatingRun,
            GatingChecksVerified = gatingVerified,
            HasVerifiedReference = hasVerifiedReference
        };
    }

    /// <summary>
    /// Misma formula que ApplicationService.CalculateProfileCompletion (15 campos) - duplicada
    /// deliberadamente para no acoplar este servicio a ApplicationService; si se agrega/quita
    /// un campo alla, hay que reflejarlo aca tambien.
    /// </summary>
    private static int CalculateProfileCompletion(PTCandidate c)
    {
        var filled = 0;
        const int total = 15;
        if (!string.IsNullOrEmpty(c.FirstName)) filled++;
        if (!string.IsNullOrEmpty(c.LastName)) filled++;
        if (!string.IsNullOrEmpty(c.Phone)) filled++;
        if (!string.IsNullOrEmpty(c.Identification)) filled++;
        if (c.BirthDate.HasValue) filled++;
        if (!string.IsNullOrEmpty(c.Country)) filled++;
        if (!string.IsNullOrEmpty(c.City)) filled++;
        if (!string.IsNullOrEmpty(c.Title)) filled++;
        if (!string.IsNullOrEmpty(c.Summary)) filled++;
        if (c.YearsOfExperience.HasValue) filled++;
        if (!string.IsNullOrEmpty(c.LinkedInUrl)) filled++;
        if (!string.IsNullOrEmpty(c.PortfolioUrl)) filled++;
        if (c.Availability.HasValue) filled++;
        if (c.WorkAuthorization.HasValue) filled++;
        if (!string.IsNullOrEmpty(c.CvUrl)) filled++;
        return (int)Math.Round((double)filled / total * 100);
    }
}
