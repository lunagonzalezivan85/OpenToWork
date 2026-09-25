using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OpenToWork.Core.Interfaces;
using OpenToWork.Models.Context;
using OpenToWork.Models.Entities;
using OpenToWork.Shared.DTOs;
using OpenToWork.Shared.Enums;

namespace OpenToWork.Core.Services;

/// <summary>
/// Invitacion al portal para empresas creadas desde el admin (decision de Darwin, 25-Sep): se crea el
/// usuario con el correo de contacto, se vincula a la empresa existente (no se crea otra, como pasaria
/// si se registrara sola en el portal) y se genera un enlace para que elija su contraseña. Reutiliza
/// el token de recuperacion de contraseña de SCUser (solo se guarda el hash) y la pagina
/// /reset-password del portal. Nadie del admin conoce la contraseña.
/// </summary>
public class CompanyPortalAccessService : ICompanyPortalAccessService
{
    private const int InviteValidityDays = 7;

    private readonly AppDbContext _context;
    private readonly ITokenCryptoService _tokenCrypto;
    private readonly IEmailService _email;
    private readonly IAuditLogService _auditLog;
    private readonly IConfiguration _config;

    public CompanyPortalAccessService(AppDbContext context, ITokenCryptoService tokenCrypto, IEmailService email,
        IAuditLogService auditLog, IConfiguration config)
    {
        _context = context;
        _tokenCrypto = tokenCrypto;
        _email = email;
        _auditLog = auditLog;
        _config = config;
    }

    public async Task<CompanyPortalAccessDto?> GetStatusAsync(Guid companyId)
    {
        var company = await _context.PT_Companies
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == companyId && !c.IsDeleted);
        if (company == null) return null;

        return new CompanyPortalAccessDto
        {
            HasAccount = company.User != null,
            IsActivated = !string.IsNullOrEmpty(company.User?.PasswordHash),
            Email = company.User?.Email ?? company.ContactEmail,
            InviteExpiresAt = company.User?.PasswordResetExpiresAt
        };
    }

    public async Task<CompanyPortalInviteResultDto?> InviteAsync(Guid companyId, Guid adminId, string? ipAddress)
    {
        var company = await _context.PT_Companies
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == companyId && !c.IsDeleted);
        if (company == null) return null;

        var user = company.User;
        if (user != null && !string.IsNullOrEmpty(user.PasswordHash))
            throw new InvalidOperationException("La empresa ya activo su acceso al portal. Si olvido la contraseña, puede recuperarla desde el login.");

        if (user == null)
        {
            var email = company.ContactEmail?.Trim();
            if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
                throw new InvalidOperationException("La empresa no tiene un correo de contacto valido. Agregalo en Editar empresa.");

            if (await _context.SC_Users.AnyAsync(u => u.Email == email && !u.IsDeleted))
                throw new InvalidOperationException($"Ya existe un usuario con el correo {email}. Usa otro correo de contacto o vincula esa cuenta manualmente.");

            user = new SCUser
            {
                Email = email,
                PasswordHash = null,
                PrimaryRole = (int)UserRole.Company,
                Phone = company.ContactPhone,
                EmailVerified = false,
                IsActive = true,
                CreatedBy = adminId
            };
            user.UserRoles.Add(new SCUserRole { Role = (int)UserRole.Company, SCUserId = user.Id });
            user.UserPreference = new SYUserPreference { SCUserId = user.Id, Theme = "navy", Language = "es" };
            _context.SC_Users.Add(user);

            company.SCUserId = user.Id;
            company.UpdatedAt = DateTime.UtcNow;
            company.UpdatedBy = adminId;
        }

        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        user.PasswordResetToken = _tokenCrypto.HashToken(token);
        user.PasswordResetExpiresAt = DateTime.UtcNow.AddDays(InviteValidityDays);
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var portalBase = (_config["Portal:BaseUrl"] ?? "http://localhost:5100/").TrimEnd('/');
        var inviteUrl = $"{portalBase}/reset-password?token={token}&invite=1";

        var html = $@"
            <p>Hola {System.Net.WebUtility.HtmlEncode(company.ContactName ?? company.Name)},</p>
            <p><strong>Trato Directo</strong> te dio acceso al portal de empresas para <strong>{System.Net.WebUtility.HtmlEncode(company.Name)}</strong>:
            ahi vas a ver el avance de tus vacantes y los candidatos verificados que te presentemos.</p>
            <p><a href=""{inviteUrl}"">Activa tu cuenta y elige tu contraseña</a> (el enlace vence en {InviteValidityDays} dias).</p>
            <p>Tu usuario es <strong>{System.Net.WebUtility.HtmlEncode(user.Email)}</strong>.</p>
            <p>Saludos,<br/>Trato Directo</p>";
        var (sent, error) = await _email.SendAsync(user.Email, company.ContactName, "Tu acceso al portal de Trato Directo", html);

        await _auditLog.LogAsync(adminId, "CompanyPortal.Invite", "PTCompany", company.Id,
            $"{{\"email\":\"{user.Email}\",\"emailSent\":{sent.ToString().ToLower()}}}", ipAddress);

        return new CompanyPortalInviteResultDto
        {
            Email = user.Email,
            InviteUrl = inviteUrl,
            ExpiresAt = user.PasswordResetExpiresAt.Value,
            EmailSent = sent,
            EmailError = sent ? null : error
        };
    }
}
