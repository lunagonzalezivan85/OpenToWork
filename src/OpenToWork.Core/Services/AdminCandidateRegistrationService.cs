using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OpenToWork.Core.Interfaces;
using OpenToWork.Core.Services;
using OpenToWork.Models.Context;
using OpenToWork.Models.Entities;
using OpenToWork.Shared.DTOs;

namespace OpenToWork.Core.Services;

public interface IAdminCandidateRegistrationService
{
    Task<AdminRegisterCandidateResultDto> RegisterFromCvAsync(byte[] fileBytes, string fileName, string mimeType, Guid adminId);
    Task<AdminRegisterCandidateResultDto> RegisterManualAsync(AdminRegisterCandidateManualDto dto, Guid adminId);
}

public class AdminCandidateRegistrationService : IAdminCandidateRegistrationService
{
    private readonly AppDbContext _context;
    private readonly ICvParserService _cvParserService;
    private readonly IProfileService _profileService;
    private readonly IConfiguration _config;

    public AdminCandidateRegistrationService(
        AppDbContext context,
        ICvParserService cvParserService,
        IProfileService profileService,
        IConfiguration config)
    {
        _context = context;
        _cvParserService = cvParserService;
        _profileService = profileService;
        _config = config;
    }

    public async Task<AdminRegisterCandidateResultDto> RegisterFromCvAsync(
        byte[] fileBytes, string fileName, string mimeType, Guid adminId)
    {
        CvParseResultDto parsedData;
        try
        {
            parsedData = await _cvParserService.ParseCvAsync(fileBytes, fileName, mimeType);
        }
        catch (Exception ex)
        {
            return new AdminRegisterCandidateResultDto
            {
                Success = false,
                Error = $"Error al parsear CV con IA: {ex.Message}"
            };
        }

        var email = parsedData.Email;
        var emailWasGenerated = false;
        if (string.IsNullOrEmpty(email))
        {
            var baseName = $"{parsedData.FirstName?.ToLower() ?? ""}{parsedData.LastName?.ToLower() ?? ""}".Replace(" ", "");
            if (string.IsNullOrEmpty(baseName))
                baseName = "candidato";
            email = $"{baseName}.{Guid.NewGuid():N}@temp.opentowork";
            emailWasGenerated = true;
        }

        var existing = await _context.SC_Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower() && !u.IsDeleted);

        if (existing != null)
            return new AdminRegisterCandidateResultDto
            {
                Success = false,
                Error = $"Ya existe un usuario con el email {email}"
            };

        var tempPassword = GenerateTempPassword();
        var user = new SCUser
        {
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(tempPassword),
            PrimaryRole = 0,
            EmailVerified = false,
            IsActive = true,
            CreatedBy = adminId
        };

        user.UserRoles.Add(new SCUserRole { Role = 0, SCUserId = user.Id });
        user.UserPreference = new SYUserPreference { SCUserId = user.Id, Theme = "navy", Language = "es" };
        user.Candidate = new PTCandidate
        {
            SCUserId = user.Id,
            WizardStep = 0,
            WizardCompleted = false,
            FirstName = parsedData.FirstName ?? "",
            LastName = parsedData.LastName ?? "",
            Phone = parsedData.Phone,
            Title = parsedData.Title,
            Summary = parsedData.Summary,
            City = parsedData.City,
            Country = parsedData.Country,
            LinkedInUrl = parsedData.LinkedInUrl,
            PortfolioUrl = parsedData.PortfolioUrl,
            YearsOfExperience = parsedData.YearsOfExperience
        };

        _context.SC_Users.Add(user);
        await _context.SaveChangesAsync();

        var cvFileName = $"cv_{user.Id}_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";
        var cvUrl = $"/uploads/cv/{cvFileName}";

        await _profileService.ApplyCvDataAsync(user.Id, parsedData, cvUrl);

        return new AdminRegisterCandidateResultDto
        {
            Success = true,
            UserId = user.Id,
            CandidateId = user.Candidate.Id,
            Email = email,
            EmailWasGenerated = emailWasGenerated,
            FullName = $"{parsedData.FirstName} {parsedData.LastName}".Trim(),
            CvUrl = cvUrl,
            ParsedData = parsedData
        };
    }

    public async Task<AdminRegisterCandidateResultDto> RegisterManualAsync(
        AdminRegisterCandidateManualDto dto, Guid adminId)
    {
        var existing = await _context.SC_Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower() && !u.IsDeleted);

        if (existing != null)
            return new AdminRegisterCandidateResultDto
            {
                Success = false,
                Error = $"Ya existe un usuario con el email {dto.Email}"
            };

        var user = new SCUser
        {
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            PrimaryRole = 0,
            EmailVerified = false,
            IsActive = true,
            CreatedBy = adminId
        };

        user.UserRoles.Add(new SCUserRole { Role = 0, SCUserId = user.Id });
        user.UserPreference = new SYUserPreference { SCUserId = user.Id, Theme = "navy", Language = "es" };
        user.Candidate = new PTCandidate
        {
            SCUserId = user.Id,
            WizardStep = 0,
            WizardCompleted = false,
            FirstName = dto.FirstName ?? "",
            LastName = dto.LastName ?? "",
            Phone = dto.Phone,
            Title = dto.Title,
            Summary = dto.Summary,
            Country = dto.Country,
            City = dto.City,
            LinkedInUrl = dto.LinkedInUrl,
            YearsOfExperience = dto.YearsOfExperience
        };

        _context.SC_Users.Add(user);
        await _context.SaveChangesAsync();

        return new AdminRegisterCandidateResultDto
        {
            Success = true,
            UserId = user.Id,
            CandidateId = user.Candidate.Id,
            Email = dto.Email,
            FullName = $"{dto.FirstName} {dto.LastName}".Trim()
        };
    }

    private static string GenerateTempPassword()
    {
        var random = new Random();
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789!@#$";
        return new string(Enumerable.Repeat(chars, 12).Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
