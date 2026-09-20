using Microsoft.EntityFrameworkCore;
using OpenToWork.Core.Interfaces;
using OpenToWork.Models.Context;
using OpenToWork.Shared.DTOs;
using OpenToWork.Shared.Enums;

namespace OpenToWork.Core.Services;

public class CompanyService : ICompanyService
{
    private readonly AppDbContext _context;

    public CompanyService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<PublicCompanyDto>> GetPublicCompaniesAsync(int? limit = null)
    {
        var query = _context.PT_Companies
            .Where(c => !c.IsDeleted && c.Status != (int)CompanyStatus.Inactiva)
            .OrderByDescending(c => c.IsFeatured)
            .ThenBy(c => c.Name)
            .Select(c => new PublicCompanyDto
            {
                Id = c.Id,
                Name = c.Name,
                LogoUrl = c.LogoUrl,
                Industry = c.Industry,
                City = c.City,
                Country = c.Country,
                IsFeatured = c.IsFeatured
            });

        if (limit is > 0)
            return await query.Take(limit.Value).ToListAsync();

        return await query.ToListAsync();
    }

    public async Task<PublicCompanyDetailDto?> GetPublicCompanyAsync(Guid id)
    {
        return await _context.PT_Companies
            .Where(c => c.Id == id && !c.IsDeleted && c.Status != (int)CompanyStatus.Inactiva)
            .Select(c => new PublicCompanyDetailDto
            {
                Id = c.Id,
                Name = c.Name,
                LogoUrl = c.LogoUrl,
                Industry = c.Industry,
                City = c.City,
                Country = c.Country,
                IsFeatured = c.IsFeatured,
                Description = c.Description,
                Website = c.Website,
                LinkedInUrl = c.LinkedInUrl,
                IsVerified = c.IsVerified,
                ActiveVacancies = c.Vacancies.Count(v => !v.IsDeleted)
            })
            .FirstOrDefaultAsync();
    }

    public async Task<MyCompanyProfileDto?> GetMyCompanyAsync(Guid userId)
    {
        return await _context.PT_Companies
            .Where(c => c.SCUserId == userId && !c.IsDeleted)
            .Select(c => new MyCompanyProfileDto
            {
                Id = c.Id,
                Name = c.Name,
                LegalName = c.LegalName,
                TaxId = c.TaxId,
                Industry = c.Industry,
                Website = c.Website,
                Description = c.Description,
                LogoUrl = c.LogoUrl,
                Country = c.Country,
                City = c.City,
                Address = c.Address,
                CompanySize = c.CompanySize,
                ContactName = c.ContactName,
                ContactEmail = c.ContactEmail,
                ContactPhone = c.ContactPhone,
                ContactPosition = c.ContactPosition,
                LinkedInUrl = c.LinkedInUrl,
                IsVerified = c.IsVerified,
                IsFeatured = c.IsFeatured
            })
            .FirstOrDefaultAsync();
    }

    public async Task<MyCompanyProfileDto?> UpdateMyCompanyAsync(Guid userId, UpdateMyCompanyProfileDto dto)
    {
        var company = await _context.PT_Companies
            .FirstOrDefaultAsync(c => c.SCUserId == userId && !c.IsDeleted);
        if (company == null) return null;

        company.Name = dto.Name;
        company.LegalName = dto.LegalName;
        company.TaxId = dto.TaxId;
        company.Industry = dto.Industry;
        company.Website = dto.Website;
        company.Description = dto.Description;
        company.LogoUrl = dto.LogoUrl;
        company.Country = dto.Country;
        company.City = dto.City;
        company.Address = dto.Address;
        company.CompanySize = dto.CompanySize;
        company.ContactName = dto.ContactName;
        company.ContactEmail = dto.ContactEmail;
        company.ContactPhone = dto.ContactPhone;
        company.ContactPosition = dto.ContactPosition;
        company.LinkedInUrl = dto.LinkedInUrl;
        company.UpdatedAt = DateTime.UtcNow;
        company.UpdatedBy = userId;

        await _context.SaveChangesAsync();
        return await GetMyCompanyAsync(userId);
    }
}
