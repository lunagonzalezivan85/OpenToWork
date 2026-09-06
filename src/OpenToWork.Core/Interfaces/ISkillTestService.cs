using OpenToWork.Shared.DTOs;

namespace OpenToWork.Core.Interfaces;

/// <summary>
/// Banco de retos tecnicos (multiple choice unicamente - ver fase-3-sub1.md). Ver plan
/// obligatorio de Fase 3 en README, sub-fase 3.6, y las decisiones documentadas en
/// docs/dsiezar/fase-3-sub6.md.
/// </summary>
public interface ISkillTestService
{
    // CRUD admin.
    Task<SkillTestAdminDto> CreateSkillTestAsync(CreateSkillTestDto dto);
    Task<List<SkillTestAdminDto>> GetAllSkillTestsAsync();
    Task<SkillTestAdminDto?> GetSkillTestByIdAsync(Guid id);
    Task<SkillTestAdminDto?> UpdateSkillTestAsync(Guid id, UpdateSkillTestDto dto);
    Task<bool> DeleteSkillTestAsync(Guid id);

    // Candidato.
    Task<List<SkillTestPublicDto>> GetAvailableTestsAsync(string? category);

    /// <summary>Idempotente: si ya hay un intento en curso sin vencer, lo devuelve en vez de crear otro (pregunta 5/8).</summary>
    Task<TestAttemptDto?> StartTestAsync(Guid candidateId, Guid testId);

    Task<TestResultDto?> SubmitTestAsync(Guid resultId, Guid candidateId, SubmitTestAnswersDto answers, int antiCheatFlags);
    Task<List<TestResultDto>> GetTestResultsAsync(Guid candidateId);
}
