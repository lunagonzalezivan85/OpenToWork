using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using OpenToWork.Core.Interfaces;
using OpenToWork.Models.Context;
using OpenToWork.Models.Entities;
using OpenToWork.Shared.DTOs;
using OpenToWork.Shared.Enums;

namespace OpenToWork.Core.Services;

/// <summary>
/// Ver plan obligatorio de Fase 3 en README, sub-fase 3.2, y las decisiones documentadas en
/// docs/dsiezar/fase-3-sub2.md (timeouts, umbrales, que cuenta como red flag, etc).
/// Solo implementa los tipos Identity/LinkedIn/Portfolio/CvCoherence - Education y Reference
/// (Type=4/5 del enum VerificationType) pertenecen a otras sub-fases (checklist de
/// investigacion existente de Iluna y sub-fase 3.5 de referencias, respectivamente).
/// </summary>
public class ValidationService : IValidationService
{
    private readonly AppDbContext _context;
    private readonly HttpClient _httpClient;

    // Gap > 6 meses entre dos experiencias consecutivas (fase-3-sub2.md, pregunta 3).
    private const int GapMonthsThreshold = 6;
    // Solapamiento > 1 mes entre dos experiencias se considera sospechoso (pregunta 4).
    private const int OverlapDaysThreshold = 30;
    // Experiencia < 3 meses (excluyendo el trabajo actual) se marca como salto laboral (pregunta 9).
    private const int ShortStintMonthsThreshold = 3;
    // Umbral de coherencia cronologica para considerar la verificacion "Verified" vs "Failed".
    private const int CoherenceVerifiedThreshold = 70;

    public ValidationService(AppDbContext context, HttpClient httpClient)
    {
        _context = context;
        _httpClient = httpClient;
    }

    public async Task<VerificationResultDto> VerifyLinkedInAsync(Guid candidateId)
    {
        var candidate = await _context.PT_Candidates.FirstOrDefaultAsync(c => c.Id == candidateId && !c.IsDeleted);
        if (candidate == null) throw new InvalidOperationException("Candidate not found");

        var url = candidate.LinkedInUrl;
        var formatValid = !string.IsNullOrWhiteSpace(url) &&
            Regex.IsMatch(url, @"^https?://([a-z]{2,3}\.)?linkedin\.com/in/[^/\s]+/?$", RegexOptions.IgnoreCase);

        var reachable = formatValid && await IsReachableAsync(url!, HttpMethod.Head, requireExact200: false);

        // Binario (fase-3-sub2.md pregunta 8): Verified solo si formato valido y alcanzable.
        var verified = formatValid && reachable;
        var result = new
        {
            hasUrl = !string.IsNullOrWhiteSpace(url),
            formatValid,
            reachable
        };

        return await SaveVerificationAsync(candidateId, VerificationType.LinkedIn,
            verified ? VerificationCheckStatus.Verified : VerificationCheckStatus.Failed,
            verified ? 100 : 0, JsonSerializer.Serialize(result));
    }

    public async Task<VerificationResultDto> VerifyPortfolioAsync(Guid candidateId)
    {
        var candidate = await _context.PT_Candidates.FirstOrDefaultAsync(c => c.Id == candidateId && !c.IsDeleted);
        if (candidate == null) throw new InvalidOperationException("Candidate not found");

        var url = candidate.PortfolioUrl;
        var hasUrl = !string.IsNullOrWhiteSpace(url);
        // Solo 200 exacto cuenta como verificado (fase-3-sub2.md pregunta 2) - 403/401/timeout
        // se marcan Failed en esta verificacion puntual, nunca lanzan ni bloquean el resto.
        var reachable = hasUrl && await IsReachableAsync(url!, HttpMethod.Get, requireExact200: true);

        var result = new { hasUrl, reachable };

        return await SaveVerificationAsync(candidateId, VerificationType.Portfolio,
            reachable ? VerificationCheckStatus.Verified : VerificationCheckStatus.Failed,
            reachable ? 100 : 0, JsonSerializer.Serialize(result));
    }

    public async Task<VerificationResultDto> VerifyIdentityAsync(Guid candidateId)
    {
        var candidate = await _context.PT_Candidates.FirstOrDefaultAsync(c => c.Id == candidateId && !c.IsDeleted);
        if (candidate == null) throw new InvalidOperationException("Candidate not found");

        // fase-3-sub2.md pregunta 5: PTCandidate no tiene todavia un campo de documento de
        // identidad subido (solo Identification, que es un numero de texto, no un archivo).
        // Se deja documentado como Pending en vez de simular una verificacion inexistente.
        var result = new { reason = "Identity document upload not implemented yet (PTCandidate has no document field)" };

        return await SaveVerificationAsync(candidateId, VerificationType.Identity,
            VerificationCheckStatus.Pending, 0, JsonSerializer.Serialize(result));
    }

    public async Task<VerificationResultDto> VerifyCvCoherenceAsync(Guid candidateId)
    {
        var candidate = await _context.PT_Candidates
            .Include(c => c.Experiences)
            .FirstOrDefaultAsync(c => c.Id == candidateId && !c.IsDeleted);
        if (candidate == null) throw new InvalidOperationException("Candidate not found");

        var experiences = candidate.Experiences
            .Where(e => !e.IsDeleted)
            .OrderBy(e => e.StartDate)
            .ToList();

        var issues = new List<string>();
        var score = 100;
        var now = DateTime.UtcNow;

        // Saltos laborales y gaps/solapamientos entre experiencias consecutivas.
        for (int i = 0; i < experiences.Count; i++)
        {
            var exp = experiences[i];
            var end = exp.IsCurrentJob || !exp.EndDate.HasValue ? now : exp.EndDate.Value;
            var months = ((end.Year - exp.StartDate.Year) * 12) + (end.Month - exp.StartDate.Month);

            if (!exp.IsCurrentJob && months < ShortStintMonthsThreshold)
            {
                issues.Add($"Salto laboral: '{exp.JobTitle}' en {exp.CompanyName} duro {months} mes(es) (< {ShortStintMonthsThreshold})");
                score -= 15;
            }

            if (i > 0)
            {
                var prev = experiences[i - 1];
                var prevEnd = prev.IsCurrentJob || !prev.EndDate.HasValue ? now : prev.EndDate.Value;
                var gapDays = (exp.StartDate - prevEnd).TotalDays;

                if (gapDays > GapMonthsThreshold * 30)
                {
                    var gapMonths = (int)(gapDays / 30);
                    // Pesa mas si el gap es reciente (dentro de los ultimos 2 anos).
                    var isRecent = (now - prevEnd).TotalDays < 730;
                    var penalty = Math.Min(30, 10 + gapMonths / 2) * (isRecent ? 1.5 : 1.0);
                    issues.Add($"Gap de {gapMonths} mes(es) entre '{prev.JobTitle}' y '{exp.JobTitle}'{(isRecent ? " (reciente)" : "")}");
                    score -= (int)penalty;
                }
                else if (gapDays < -OverlapDaysThreshold)
                {
                    issues.Add($"Solapamiento entre '{prev.JobTitle}' y '{exp.JobTitle}' (> {OverlapDaysThreshold} dias)");
                    score -= 10;
                }
            }
        }

        score = Math.Clamp(score, 0, 100);
        var verified = score >= CoherenceVerifiedThreshold;

        var result = new { issues, score };

        return await SaveVerificationAsync(candidateId, VerificationType.CvCoherence,
            verified ? VerificationCheckStatus.Verified : VerificationCheckStatus.Failed,
            score, JsonSerializer.Serialize(result));
    }

    public async Task<List<string>> DetectRedFlagsAsync(Guid candidateId)
    {
        var candidate = await _context.PT_Candidates
            .Include(c => c.Experiences)
            .FirstOrDefaultAsync(c => c.Id == candidateId && !c.IsDeleted);
        if (candidate == null) throw new InvalidOperationException("Candidate not found");

        var flags = new List<string>();
        var now = DateTime.UtcNow;
        var experiences = candidate.Experiences.Where(e => !e.IsDeleted).OrderBy(e => e.StartDate).ToList();

        foreach (var exp in experiences)
        {
            if (exp.IsCurrentJob) continue;
            var end = exp.EndDate ?? now;
            var months = ((end.Year - exp.StartDate.Year) * 12) + (end.Month - exp.StartDate.Month);
            if (months < ShortStintMonthsThreshold)
                flags.Add($"Salto laboral: '{exp.JobTitle}' en {exp.CompanyName} ({months} mes(es))");
        }

        for (int i = 1; i < experiences.Count; i++)
        {
            var prevEnd = experiences[i - 1].IsCurrentJob || !experiences[i - 1].EndDate.HasValue ? now : experiences[i - 1].EndDate!.Value;
            var gapDays = (experiences[i].StartDate - prevEnd).TotalDays;
            if (gapDays > GapMonthsThreshold * 30)
                flags.Add($"Gap inexplicado de {(int)(gapDays / 30)} mes(es) antes de '{experiences[i].JobTitle}'");
        }

        // Cambios de sector frecuentes: fuera de alcance (fase-3-sub2.md pregunta 9) - no existe
        // un campo de industria/sector en PTCandidateExperience todavia.

        return flags;
    }

    public async Task<List<VerificationResultDto>> RunAllVerificationsAsync(Guid candidateId)
    {
        var results = new List<VerificationResultDto>
        {
            await VerifyIdentityAsync(candidateId),
            await VerifyLinkedInAsync(candidateId),
            await VerifyPortfolioAsync(candidateId),
            await VerifyCvCoherenceAsync(candidateId)
        };

        return results;
    }

    public async Task<List<VerificationResultDto>> GetVerificationsAsync(Guid candidateId)
    {
        // Incluye Type=Reference (lo escribe ReferenceService, no este servicio) para que la
        // lista sea completa - ver fase-3-sub8.md.
        var verifications = await _context.PT_Verifications
            .Where(v => v.PT_CandidateId == candidateId && !v.IsDeleted)
            .OrderBy(v => v.Type)
            .ToListAsync();

        return verifications.Select(v => new VerificationResultDto
        {
            Type = v.Type,
            Status = v.Status,
            Score = v.Score,
            Result = v.Result,
            VerifiedAt = v.VerifiedAt
        }).ToList();
    }

    private async Task<bool> IsReachableAsync(string url, HttpMethod method, bool requireExact200)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || (uri.Scheme != "http" && uri.Scheme != "https"))
            return false;

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            using var request = new HttpRequestMessage(method, uri);
            var response = await _httpClient.SendAsync(request, cts.Token);
            return requireExact200 ? response.StatusCode == System.Net.HttpStatusCode.OK : response.IsSuccessStatusCode;
        }
        catch
        {
            // Timeout, DNS, TLS, lo que sea - nunca debe tumbar el flujo del candidato.
            return false;
        }
    }

    public async Task<VerificationResultDto> SetVerificationStatusAsync(Guid candidateId, int type, int status, Guid adminId)
    {
        var checkStatus = (VerificationCheckStatus)status;
        if (checkStatus != VerificationCheckStatus.Verified && checkStatus != VerificationCheckStatus.Failed)
            throw new InvalidOperationException("Status must be Verified or Failed for a manual override");

        var score = checkStatus == VerificationCheckStatus.Verified ? 100 : 0;
        var result = JsonSerializer.Serialize(new { manualOverride = true, adminId, at = DateTime.UtcNow });
        return await SaveVerificationAsync(candidateId, (VerificationType)type, checkStatus, score, result);
    }

    private async Task<VerificationResultDto> SaveVerificationAsync(Guid candidateId, VerificationType type, VerificationCheckStatus status, int score, string resultJson)
    {
        var verification = await _context.PT_Verifications
            .FirstOrDefaultAsync(v => v.PT_CandidateId == candidateId && v.Type == (int)type && !v.IsDeleted);

        if (verification == null)
        {
            verification = new PTVerification { PT_CandidateId = candidateId, Type = (int)type };
            _context.PT_Verifications.Add(verification);
        }

        verification.Status = (int)status;
        verification.Score = score;
        verification.Result = resultJson;
        verification.VerifiedAt = DateTime.UtcNow;
        verification.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new VerificationResultDto
        {
            Type = (int)type,
            Status = (int)status,
            Score = score,
            Result = resultJson,
            VerifiedAt = verification.VerifiedAt
        };
    }
}
