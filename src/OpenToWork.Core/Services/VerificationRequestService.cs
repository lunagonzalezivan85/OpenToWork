using Microsoft.EntityFrameworkCore;
using OpenToWork.Core.Interfaces;
using OpenToWork.Models.Context;
using OpenToWork.Models.Entities;
using OpenToWork.Shared.DTOs;

namespace OpenToWork.Core.Services;

public class VerificationRequestService : IVerificationRequestService
{
    private const long MaxFileBytes = 5 * 1024 * 1024; // 5MB
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        { ".jpg", ".jpeg", ".png", ".webp", ".pdf" };

    private readonly AppDbContext _context;

    public VerificationRequestService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<VerificationRequestResultDto?> SubmitAsync(Guid userId, SubmitVerificationRequestDto dto, string uploadsRoot, string? consentIp)
    {
        var candidate = await _context.PT_Candidates
            .FirstOrDefaultAsync(c => c.SCUserId == userId && !c.IsDeleted);
        if (candidate == null) return null;

        var request = new PTVerificationRequest
        {
            PT_CandidateId = candidate.Id,
            ReferenceNumber = GenerateReferenceNumber(),
            HasSpanishNationality = dto.HasSpanishNationality,
            AdminSituation = dto.AdminSituation,
            FullName = dto.FullName?.Trim(),
            DocumentNumber = dto.DocumentNumber?.Trim().ToUpperInvariant(),
            DocumentExpiry = dto.DocumentExpiry,
            Nationality = dto.Nationality?.Trim(),
            PermitType = dto.PermitType,
            Phone = dto.Phone?.Trim(),
            ContactTimePreference = dto.ContactTimePreference,
            Status = 0,
            ConsentAcceptedAt = DateTime.UtcNow,
            ConsentIp = consentIp,
            TermsVersion = "1.0",
            CreatedBy = userId
        };

        // Rama 4 (sin permiso): solo datos de contacto, sin documentos (minimizacion RGPD).
        if (dto.AdminSituation != 3 && dto.Documents.Count > 0)
        {
            var requestDir = Path.Combine(uploadsRoot, "verification", request.Id.ToString());
            Directory.CreateDirectory(requestDir);

            foreach (var doc in dto.Documents)
            {
                var ext = Path.GetExtension(doc.FileName);
                if (string.IsNullOrEmpty(ext) || !AllowedExtensions.Contains(ext)) continue;

                byte[] bytes;
                try { bytes = Convert.FromBase64String(doc.DataBase64); }
                catch { continue; }
                if (bytes.Length == 0 || bytes.Length > MaxFileBytes) continue;

                var safeSlot = string.Concat(doc.Slot.Where(char.IsLetterOrDigit));
                if (string.IsNullOrEmpty(safeSlot)) continue;

                var fileName = $"{safeSlot}_{Guid.NewGuid():N}{ext.ToLowerInvariant()}";
                var fullPath = Path.Combine(requestDir, fileName);
                await File.WriteAllBytesAsync(fullPath, bytes);

                var relativePath = Path.Combine("uploads", "verification", request.Id.ToString(), fileName);
                switch (safeSlot)
                {
                    case "front": request.DocFrontPath = relativePath; break;
                    case "back": request.DocBackPath = relativePath; break;
                    case "extra1": request.DocExtra1Path = relativePath; break;
                    case "extra2": request.DocExtra2Path = relativePath; break;
                }
            }
        }

        _context.PT_VerificationRequests.Add(request);
        await _context.SaveChangesAsync();

        return ToDto(request);
    }

    public async Task<VerificationRequestResultDto?> GetLatestAsync(Guid userId)
    {
        var candidate = await _context.PT_Candidates
            .FirstOrDefaultAsync(c => c.SCUserId == userId && !c.IsDeleted);
        if (candidate == null) return null;

        return await _context.PT_VerificationRequests
            .Where(r => r.PT_CandidateId == candidate.Id && !r.IsDeleted)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new VerificationRequestResultDto
            {
                Id = r.Id,
                ReferenceNumber = r.ReferenceNumber,
                Status = r.Status,
                AdminSituation = r.AdminSituation,
                CreatedAt = r.CreatedAt
            })
            .FirstOrDefaultAsync();
    }

    private static VerificationRequestResultDto ToDto(PTVerificationRequest r) => new()
    {
        Id = r.Id,
        ReferenceNumber = r.ReferenceNumber,
        Status = r.Status,
        AdminSituation = r.AdminSituation,
        CreatedAt = r.CreatedAt
    };

    private static string GenerateReferenceNumber()
        => $"TD-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
}
