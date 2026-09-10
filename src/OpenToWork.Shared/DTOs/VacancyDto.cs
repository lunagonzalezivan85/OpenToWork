namespace OpenToWork.Shared.DTOs;

public class VacancyDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string? CompanyLogoUrl { get; set; }
    public bool CompanyIsVerified { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Requirements { get; set; }
    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }
    public string? Location { get; set; }
    public int ContractType { get; set; }
    public int WorkMode { get; set; }
    public string? Category { get; set; }
    public int? ExperienceLevel { get; set; }
    public int? EnglishLevel { get; set; }
    public int? RequiredApplicants { get; set; }
    public int? YearsExperience { get; set; }
    public int Status { get; set; }
    public DateTime? PublishedAt { get; set; }
    public int ViewsCount { get; set; }
}

public class CreateVacancyDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Requirements { get; set; }
    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }
    public string? Location { get; set; }
    public int ContractType { get; set; }
    public int WorkMode { get; set; }
    public string? Category { get; set; }
    public int? ExperienceLevel { get; set; }
    public int? EnglishLevel { get; set; }
    public int? RequiredApplicants { get; set; }
    public int? YearsExperience { get; set; }
}

public class UpdateVacancyDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Requirements { get; set; }
    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }
    public string? Location { get; set; }
    public int? ContractType { get; set; }
    public int? WorkMode { get; set; }
    public string? Category { get; set; }
    public int? ExperienceLevel { get; set; }
    public int? EnglishLevel { get; set; }
    public int? Status { get; set; }
}

public class SearchPermanentVacancyDto
{
    public string? Query { get; set; }
    public string? Location { get; set; }
    public int? ContractType { get; set; }
    public int? WorkMode { get; set; }
    public string? Category { get; set; }
    public int? ExperienceLevel { get; set; }
    public int? EnglishLevel { get; set; }
    public decimal? SalaryMin { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
