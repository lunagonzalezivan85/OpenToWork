using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OpenToWork.Core.Interfaces;
using OpenToWork.Models.Context;
using OpenToWork.Models.Entities;
using OpenToWork.Shared.DTOs;
using OpenToWork.Shared.Enums;

namespace OpenToWork.Core.Services;

public class AdminContractService : IAdminContractService
{
    private readonly AppDbContext _context;
    private readonly IAuditLogService _auditLog;
    private readonly IJobPricingService _pricing;
    private readonly IPromoCodeService _promoCodes;
    private readonly IContractPaymentService _payments;
    private readonly ICompanyCrmService _companyCrm;
    private readonly IEmailService _email;

    public AdminContractService(AppDbContext context, IAuditLogService auditLog, IJobPricingService pricing, IPromoCodeService promoCodes, IContractPaymentService payments, ICompanyCrmService companyCrm, IEmailService email)
    {
        _context = context;
        _auditLog = auditLog;
        _pricing = pricing;
        _promoCodes = promoCodes;
        _payments = payments;
        _companyCrm = companyCrm;
        _email = email;
    }

    public async Task<AdminVacancyContractDto?> GetByIdAsync(Guid contractId)
    {
        return await ProjectContractAsync(c => c.Id == contractId && !c.IsDeleted);
    }

    public async Task<AdminVacancyContractDto?> GetByCompanyAsync(Guid companyId)
    {
        return await ProjectContractAsync(c => c.PT_CompanyId == companyId && !c.IsDeleted);
    }

    private async Task<AdminVacancyContractDto?> ProjectContractAsync(System.Linq.Expressions.Expression<Func<PTVacancyContract, bool>> predicate)
    {
        return await _context.PT_VacancyContracts
            .Where(predicate)
            .Select(c => new AdminVacancyContractDto
            {
                Id = c.Id,
                CompanyId = c.PT_CompanyId,
                CompanyName = c.Company.Name,
                CompanyLegalName = c.Company.LegalName,
                CompanyTaxId = c.Company.TaxId,
                CompanyAddress = c.Company.Address,
                CompanyCountry = c.Company.Country,
                CompanyCity = c.Company.City,
                CompanyContactName = c.Company.ContactName,
                CompanyContactPosition = c.Company.ContactPosition,
                CompanyContactDniNie = c.Company.ContactDniNie,
                CompanyContactEmail = c.Company.ContactEmail,
                CompanyContactPhone = c.Company.ContactPhone,
                ContractNumber = c.ContractNumber,
                Status = c.Status,
                Vacancies = c.ContractVacancies
                    .Where(cv => !cv.IsDeleted)
                    .Select(cv => new ContractVacancyItemDto
                    {
                        VacancyId = cv.PT_VacancyId,
                        Title = cv.Vacancy!.Title,
                        Description = cv.Vacancy.Description,
                        Requirements = cv.Vacancy.Requirements,
                        Location = cv.Vacancy.Location,
                        Category = cv.Vacancy.Category,
                        ContractType = cv.Vacancy.ContractType,
                        WorkMode = cv.Vacancy.WorkMode,
                        SalaryMin = cv.Vacancy.SalaryMin,
                        SalaryMax = cv.Vacancy.SalaryMax,
                        RequiredApplicants = cv.Vacancy.RequiredApplicants,
                        YearsExperience = cv.Vacancy.YearsExperience,
                        JobTypeId = cv.PT_JobTypeId,
                        JobTypeName = cv.JobType != null ? cv.JobType.Name : null,
                        JobLevelName = cv.JobType != null ? cv.JobType.JobLevel.Name : null,
                        BasePrice = cv.BasePrice,
                        PromoCode = cv.PromoCodeText,
                        DiscountAmount = cv.DiscountAmount,
                        FinalPrice = cv.FinalPrice,
                        IsManualOverride = cv.IsManualOverride,
                        OverrideReason = cv.OverrideReason,
                        WarrantyDays = cv.WarrantyDays
                    }).ToList(),
                ScopeServices = c.ScopeServices == null
                    ? new List<string>()
                    : System.Text.Json.JsonSerializer.Deserialize<List<string>>(c.ScopeServices) ?? new List<string>(),
                TargetCandidates = c.TargetCandidates,
                JobTypeCategory = c.JobTypeCategory,
                TargetCoverageDays = c.TargetCoverageDays,
                FeeAmount = c.FeeAmount,
                Currency = c.Currency,
                FeeApplicationType = c.FeeApplicationType,
                PaymentOpeningPct = c.PaymentOpeningPct,
                PaymentValidationPct = c.PaymentValidationPct,
                PaymentConsolidationPct = c.PaymentConsolidationPct,
                FeeExceptions = c.FeeExceptions,
                AcceptedAt = c.AcceptedAt,
                RejectedAt = c.RejectedAt,
                RejectionReason = c.RejectionReason,
                CreatedAt = c.CreatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<AdminVacancyContractDto?> CreateAsync(Guid companyId, AdminSaveVacancyContractDto dto, Guid adminId, string? ipAddress)
    {
        NormalizePaymentPercentages(dto);
        if (dto.PaymentOpeningPct + dto.PaymentValidationPct + dto.PaymentConsolidationPct != 100m)
            throw new InvalidOperationException("La distribucion de pago (Apertura + Validacion + Consolidacion) debe sumar 100%.");
        if (dto.VacancyLines.Count == 0)
            throw new InvalidOperationException("El contrato debe incluir al menos una vacante.");

        var company = await _context.PT_Companies.FirstOrDefaultAsync(c => c.Id == companyId && !c.IsDeleted);
        if (company == null) return null;

        var contract = new PTVacancyContract
        {
            PT_CompanyId = companyId,
            ContractNumber = await GenerateContractNumberAsync(),
            Status = (int)ContractStatus.Draft,
            CreatedBy = adminId
        };
        ApplyDtoToContract(contract, dto, adminId);

        var appliedPromos = new List<(Guid PromoCodeId, PTContractVacancy Line)>();
        foreach (var line in DistinctLines(dto.VacancyLines))
        {
            var cv = new PTContractVacancy { PT_VacancyId = line.VacancyId, CreatedBy = adminId };
            await ApplyPricingAsync(cv, line, appliedPromos);
            contract.ContractVacancies.Add(cv);
        }
        contract.FeeAmount = contract.ContractVacancies.Sum(cv => cv.FinalPrice ?? 0);

        _context.PT_VacancyContracts.Add(contract);
        await _context.SaveChangesAsync();
        await RedeemPromosAsync(appliedPromos);
        await _auditLog.LogAsync(adminId, "CreateContract",
            "PT_VacancyContracts", contract.Id, $"{{\"contractNumber\":\"{contract.ContractNumber}\"}}", ipAddress);

        // Generar el contrato de servicio es, en terminos del pipeline comercial, presentarle una
        // propuesta formal a la empresa - si el trato todavia no llego ahi, avanza la etapa. No
        // retrocede un trato que ya esta en Negociacion/Cerrado (ganado o perdido).
        await AdvancePipelineStageIfEarlierAsync(companyId, CompanyPipelineStage.PropuestaEnviada,
            "Contrato de servicio generado.", adminId, ipAddress);

        return await GetByIdAsync(contract.Id);
    }

    public async Task<AdminVacancyContractDto?> SaveAsync(Guid contractId, AdminSaveVacancyContractDto dto, Guid adminId, string? ipAddress)
    {
        NormalizePaymentPercentages(dto);
        if (dto.PaymentOpeningPct + dto.PaymentValidationPct + dto.PaymentConsolidationPct != 100m)
            throw new InvalidOperationException("La distribucion de pago (Apertura + Validacion + Consolidacion) debe sumar 100%.");
        if (dto.VacancyLines.Count == 0)
            throw new InvalidOperationException("El contrato debe incluir al menos una vacante.");

        var contract = await _context.PT_VacancyContracts
            .Include(c => c.ContractVacancies)
            .FirstOrDefaultAsync(c => c.Id == contractId && !c.IsDeleted);

        if (contract == null) return null;
        if (contract.Status != (int)ContractStatus.Draft && contract.Status != (int)ContractStatus.Rejected)
            throw new InvalidOperationException("Solo se puede editar un contrato en Borrador o Rechazado.");

        if (contract.Status == (int)ContractStatus.Rejected)
        {
            contract.Status = (int)ContractStatus.Draft;
            contract.RejectedAt = null;
            contract.RejectionReason = null;
        }

        ApplyDtoToContract(contract, dto, adminId);

        // Sync vacancy links: remove deleted, add new, re-cotizar las que se mantienen
        var lines = DistinctLines(dto.VacancyLines).ToDictionary(l => l.VacancyId);
        var appliedPromos = new List<(Guid PromoCodeId, PTContractVacancy Line)>();

        foreach (var cv in contract.ContractVacancies.Where(cv => !cv.IsDeleted && !lines.ContainsKey(cv.PT_VacancyId)))
        {
            cv.IsDeleted = true;
            cv.DeletedAt = DateTime.UtcNow;
            cv.DeletedBy = adminId;
        }

        var existing = contract.ContractVacancies.Where(cv => !cv.IsDeleted).ToDictionary(cv => cv.PT_VacancyId);
        foreach (var (vacancyId, line) in lines)
        {
            if (existing.TryGetValue(vacancyId, out var cv))
            {
                cv.UpdatedAt = DateTime.UtcNow;
                cv.UpdatedBy = adminId;
            }
            else
            {
                cv = new PTContractVacancy { PT_VacancyId = vacancyId, CreatedBy = adminId };
                contract.ContractVacancies.Add(cv);
            }
            await ApplyPricingAsync(cv, line, appliedPromos);
        }
        contract.FeeAmount = contract.ContractVacancies.Where(cv => !cv.IsDeleted).Sum(cv => cv.FinalPrice ?? 0);

        await _context.SaveChangesAsync();
        await RedeemPromosAsync(appliedPromos);
        await _auditLog.LogAsync(adminId, "UpdateContract",
            "PT_VacancyContracts", contract.Id, $"{{\"contractNumber\":\"{contract.ContractNumber}\"}}", ipAddress);

        return await GetByIdAsync(contractId);
    }

    private static IEnumerable<ContractVacancyLineDto> DistinctLines(List<ContractVacancyLineDto> lines) =>
        lines.GroupBy(l => l.VacancyId).Select(g => g.First());

    /// <summary>Redondea a 2 decimales antes de validar que sumen 100% y de guardar, para que
    /// un valor con mas precision del lado del cliente (ej. pegado o de un stepper distinto)
    /// no rompa la suma exacta ni quede guardado con decimales de mas.</summary>
    private static void NormalizePaymentPercentages(AdminSaveVacancyContractDto dto)
    {
        dto.PaymentOpeningPct = Math.Round(dto.PaymentOpeningPct, 2);
        dto.PaymentValidationPct = Math.Round(dto.PaymentValidationPct, 2);
        dto.PaymentConsolidationPct = Math.Round(dto.PaymentConsolidationPct, 2);
    }

    /// <summary>Resuelve el precio de una linea del contrato: manual (con motivo) o automatico
    /// (precio de lista del tipo de puesto de la vacante + codigo promocional opcional).</summary>
    private async Task ApplyPricingAsync(PTContractVacancy cv, ContractVacancyLineDto line, List<(Guid PromoCodeId, PTContractVacancy Line)> appliedPromos)
    {
        var vacancy = await _context.PT_Vacancies
            .FirstOrDefaultAsync(v => v.Id == line.VacancyId && !v.IsDeleted)
            ?? throw new InvalidOperationException("Una de las vacantes seleccionadas ya no existe.");

        cv.PT_JobTypeId = vacancy.PT_JobTypeId;
        cv.WarrantyDays = await ResolveWarrantyDaysAsync(line, vacancy);

        if (line.ManualPrice.HasValue)
        {
            if (string.IsNullOrWhiteSpace(line.OverrideReason))
                throw new InvalidOperationException($"\"{vacancy.Title}\": para fijar un precio manual hay que indicar el motivo.");
            if (line.ManualPrice.Value < 0)
                throw new InvalidOperationException($"\"{vacancy.Title}\": el precio manual no puede ser negativo.");

            var manualPrice = Math.Round(line.ManualPrice.Value, 2);
            cv.BasePrice = manualPrice;
            cv.PT_PromoCodeId = null;
            cv.PromoCodeText = null;
            cv.DiscountAmount = 0;
            cv.FinalPrice = manualPrice;
            cv.IsManualOverride = true;
            cv.OverrideReason = line.OverrideReason!.Trim();
            return;
        }

        if (vacancy.PT_JobTypeId == null)
            throw new InvalidOperationException($"\"{vacancy.Title}\" no tiene un tipo de puesto asignado. Asignale uno o escribe un precio manual con motivo.");

        var price = await _pricing.GetActivePriceAsync(vacancy.PT_JobTypeId.Value)
            ?? throw new InvalidOperationException($"\"{vacancy.Title}\": su tipo de puesto todavia no tiene un precio de lista definido.");

        cv.BasePrice = price.BasePrice;
        cv.IsManualOverride = false;
        cv.OverrideReason = null;

        if (!string.IsNullOrWhiteSpace(line.PromoCode))
        {
            var validation = await _promoCodes.ValidateAsync(line.PromoCode, line.VacancyId);
            if (!validation.IsValid)
                throw new InvalidOperationException($"\"{vacancy.Title}\": codigo promocional invalido - {validation.ErrorMessage}");

            cv.PT_PromoCodeId = validation.PromoCodeId;
            cv.PromoCodeText = validation.Code;
            cv.DiscountAmount = validation.DiscountAmount;
            cv.FinalPrice = validation.FinalAmount;
            appliedPromos.Add((validation.PromoCodeId!.Value, cv));
        }
        else
        {
            cv.PT_PromoCodeId = null;
            cv.PromoCodeText = null;
            cv.DiscountAmount = 0;
            cv.FinalPrice = price.BasePrice;
        }
    }

    /// <summary>Garantia de esta vacante: explicita si el admin la fijo en la linea, si no la
    /// referencia del nivel de su tipo de puesto (PTJobLevel.WarrantyDays) - null si la vacante
    /// no tiene tipo de puesto asignado y tampoco se fijo un valor explicito.</summary>
    private async Task<int?> ResolveWarrantyDaysAsync(ContractVacancyLineDto line, PTVacancy vacancy)
    {
        if (line.WarrantyDays.HasValue) return line.WarrantyDays;
        if (vacancy.PT_JobTypeId == null) return null;

        return await _context.PT_JobTypes
            .Where(t => t.Id == vacancy.PT_JobTypeId.Value && !t.IsDeleted)
            .Select(t => t.JobLevel.WarrantyDays)
            .FirstOrDefaultAsync();
    }

    private async Task RedeemPromosAsync(List<(Guid PromoCodeId, PTContractVacancy Line)> appliedPromos)
    {
        foreach (var (promoCodeId, line) in appliedPromos)
        {
            _context.PT_PromoCodeRedemptions.Add(new PTPromoCodeRedemption
            {
                PT_PromoCodeId = promoCodeId,
                PT_ContractVacancyId = line.Id,
                DiscountAmountApplied = line.DiscountAmount,
                CreatedBy = line.CreatedBy
            });
            await _promoCodes.IncrementUsageAsync(promoCodeId);
        }
        if (appliedPromos.Count > 0)
            await _context.SaveChangesAsync();
    }

    private void ApplyDtoToContract(PTVacancyContract contract, AdminSaveVacancyContractDto dto, Guid adminId)
    {
        contract.ScopeServices = dto.ScopeServices.Count == 0
            ? null
            : System.Text.Json.JsonSerializer.Serialize(dto.ScopeServices);
        contract.TargetCandidates = dto.TargetCandidates;
        contract.JobTypeCategory = dto.JobTypeCategory;
        contract.TargetCoverageDays = dto.TargetCoverageDays;
        // WarrantyDays ya no es del contrato: es por vacante (ver PTContractVacancy.WarrantyDays,
        // resuelto en ApplyPricingAsync/ResolveWarrantyDaysAsync).
        // FeeAmount ya no se recibe del formulario: se recalcula como suma de las lineas (ver CreateAsync/SaveAsync).
        contract.Currency = string.IsNullOrWhiteSpace(dto.Currency) ? "EUR" : dto.Currency.Trim().ToUpperInvariant();
        contract.FeeApplicationType = dto.FeeApplicationType;
        contract.PaymentOpeningPct = dto.PaymentOpeningPct;
        contract.PaymentValidationPct = dto.PaymentValidationPct;
        contract.PaymentConsolidationPct = dto.PaymentConsolidationPct;
        contract.FeeExceptions = dto.FeeExceptions;
        contract.UpdatedAt = DateTime.UtcNow;
        contract.UpdatedBy = adminId;
    }

    /// <summary>Marca el anexo como enviado a la empresa (Draft -> Sent) y, si SMTP esta
    /// configurado y habilitado, notifica por correo al contacto de la empresa. El envio de
    /// correo nunca bloquea el cambio de estado: si falla, queda igual marcado como Sent
    /// (la fuente de verdad es el estado en BD, no la entrega del correo).</summary>
    public async Task<bool> SendAsync(Guid contractId, Guid adminId, string? ipAddress)
    {
        var contract = await _context.PT_VacancyContracts
            .FirstOrDefaultAsync(c => c.Id == contractId && !c.IsDeleted);
        if (contract == null || contract.Status != (int)ContractStatus.Draft) return false;

        contract.Status = (int)ContractStatus.Sent;
        contract.UpdatedAt = DateTime.UtcNow;
        contract.UpdatedBy = adminId;

        await _context.SaveChangesAsync();
        await _auditLog.LogAsync(adminId, "SendContract", "PT_VacancyContracts", contract.Id,
            $"{{\"contractNumber\":\"{contract.ContractNumber}\"}}", ipAddress);

        await NotifyCompanyContractSentAsync(contract, adminId, ipAddress);
        return true;
    }

    private async Task NotifyCompanyContractSentAsync(PTVacancyContract contract, Guid adminId, string? ipAddress)
    {
        var company = await _context.PT_Companies
            .FirstOrDefaultAsync(c => c.Id == contract.PT_CompanyId && !c.IsDeleted);
        if (company == null || string.IsNullOrWhiteSpace(company.ContactEmail))
            return;

        var subject = $"Contrato de servicio Trato Directo - {contract.ContractNumber}";
        var feeText = contract.FeeAmount.HasValue ? $"{contract.FeeAmount.Value:0.00} EUR" : "a confirmar";
        var html = $@"
            <p>Hola {System.Net.WebUtility.HtmlEncode(company.ContactName ?? company.Name)},</p>
            <p>El contrato de servicio <strong>{System.Net.WebUtility.HtmlEncode(contract.ContractNumber)}</strong>
            con <strong>Trato Directo</strong> para <strong>{System.Net.WebUtility.HtmlEncode(company.Name)}</strong>
            ya esta listo. Importe: <strong>{feeText}</strong>.</p>
            <p>Tu ejecutivo de cuenta se pondra en contacto para los siguientes pasos.</p>
            <p>Saludos,<br/>Trato Directo</p>";

        var (success, error) = await _email.SendAsync(company.ContactEmail, company.ContactName, subject, html);
        await _auditLog.LogAsync(adminId, success ? "ContractEmailSent" : "ContractEmailFailed",
            "PT_VacancyContracts", contract.Id,
            success ? null : $"{{\"error\":\"{error?.Replace("\"", "'")}\"}}", ipAddress);
    }

    public async Task<bool> DecideAsync(Guid contractId, bool accepted, string? reason, Guid adminId, string? ipAddress)
    {
        var contract = await _context.PT_VacancyContracts
            .FirstOrDefaultAsync(c => c.Id == contractId && !c.IsDeleted);
        if (contract == null) return false;

        if (contract.Status != (int)ContractStatus.Sent && contract.Status != (int)ContractStatus.Draft) return false;

        if (accepted)
        {
            contract.Status = (int)ContractStatus.Accepted;
            contract.AcceptedAt = DateTime.UtcNow;
        }
        else
        {
            if (string.IsNullOrWhiteSpace(reason)) return false;
            contract.Status = (int)ContractStatus.Rejected;
            contract.RejectedAt = DateTime.UtcNow;
            contract.RejectionReason = reason.Trim();
        }
        contract.UpdatedAt = DateTime.UtcNow;
        contract.UpdatedBy = adminId;

        await _context.SaveChangesAsync();
        await _auditLog.LogAsync(adminId, accepted ? "AcceptContract" : "RejectContract",
            "PT_VacancyContracts", contract.Id,
            $"{{\"contractNumber\":\"{contract.ContractNumber}\",\"accepted\":{accepted.ToString().ToLower()}}}", ipAddress);

        if (accepted)
        {
            await _payments.CreateTranchesForContractAsync(contract.Id);

            // La aceptacion del contrato es la senal definitiva de que el trato se gano.
            await SetPipelineStageAsync(contract.PT_CompanyId, CompanyPipelineStage.CerradoGanado,
                "Contrato de servicio aceptado por la empresa.", adminId, ipAddress);
        }

        return true;
    }

    /// <summary>Busca el pipeline comercial activo de la empresa (no hay FK directa entre
    /// PTVacancyContract y PTCompanyPipeline, ambos cuelgan de PT_CompanyId). Devuelve null si
    /// la empresa no tiene un pipeline (dato legacy/de prueba) - no es un error, solo no hay
    /// nada que mover.</summary>
    private async Task<PTCompanyPipeline?> GetActivePipelineAsync(Guid companyId)
    {
        return await _context.PT_CompanyPipelines
            .Where(p => p.PT_CompanyId == companyId && !p.IsDeleted && !p.IsDismissed)
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync();
    }

    /// <summary>Avanza el pipeline a targetStage solo si esta en una etapa anterior - nunca
    /// retrocede un trato que ya esta en Negociacion o Cerrado (ganado o perdido).</summary>
    private async Task AdvancePipelineStageIfEarlierAsync(Guid companyId, CompanyPipelineStage targetStage, string note, Guid adminId, string? ipAddress)
    {
        var pipeline = await GetActivePipelineAsync(companyId);
        if (pipeline == null || pipeline.CurrentStage >= (int)targetStage) return;

        await _companyCrm.MoveStageAsync(pipeline.Id, new CompanyMoveStageDto { ToStage = (int)targetStage, Notes = note }, adminId, ipAddress);
    }

    /// <summary>Fija el pipeline en targetStage sin importar en que etapa estaba (salvo que ya
    /// este ahi, para no duplicar el registro en el historial de etapas).</summary>
    private async Task SetPipelineStageAsync(Guid companyId, CompanyPipelineStage targetStage, string note, Guid adminId, string? ipAddress)
    {
        var pipeline = await GetActivePipelineAsync(companyId);
        if (pipeline == null || pipeline.CurrentStage == (int)targetStage) return;

        await _companyCrm.MoveStageAsync(pipeline.Id, new CompanyMoveStageDto { ToStage = (int)targetStage, Notes = note }, adminId, ipAddress);
    }

    private async Task<string> GenerateContractNumberAsync()
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"TD-{year}-";
        var last = await _context.PT_VacancyContracts
            .Where(c => c.ContractNumber.StartsWith(prefix))
            .OrderByDescending(c => c.ContractNumber)
            .Select(c => c.ContractNumber)
            .FirstOrDefaultAsync();

        var next = 1;
        if (last != null && int.TryParse(last[prefix.Length..], out var seq)) next = seq + 1;
        return $"{prefix}{next:D4}";
    }
}
