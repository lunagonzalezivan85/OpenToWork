using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OpenToWork.Core.Interfaces;
using OpenToWork.Models.Context;
using OpenToWork.Models.Entities;
using OpenToWork.Shared.DTOs;

namespace OpenToWork.Core.Services;

/// <summary>
/// Ver plan obligatorio de Fase 3 en README, sub-fase 3.6, y las decisiones documentadas en
/// docs/dsiezar/fase-3-sub6.md (por que solo multiple choice, 1 intento, etc).
/// </summary>
public class SkillTestService : ISkillTestService
{
    private readonly AppDbContext _context;
    private readonly IScoringService _scoringService;

    // Umbral para que el 6to componente de EvidenceIndex cuente (fase-3-sub6.md pregunta 6).
    private const int PassingScore = 60;

    public SkillTestService(AppDbContext context, IScoringService scoringService)
    {
        _context = context;
        _scoringService = scoringService;
    }

    public async Task<SkillTestAdminDto> CreateSkillTestAsync(CreateSkillTestDto dto)
    {
        var test = new PTSkillTest
        {
            Category = dto.Category,
            Difficulty = dto.Difficulty,
            Title = dto.Title,
            Description = dto.Description,
            TimeLimit = dto.TimeLimit,
            Questions = JsonSerializer.Serialize(dto.Questions),
            IsActive = dto.IsActive
        };

        _context.PT_SkillTests.Add(test);
        await _context.SaveChangesAsync();
        return ToAdminDto(test);
    }

    public async Task<List<SkillTestAdminDto>> GetAllSkillTestsAsync()
    {
        var tests = await _context.PT_SkillTests.Where(t => !t.IsDeleted).OrderBy(t => t.Category).ThenBy(t => t.Title).ToListAsync();
        return tests.Select(ToAdminDto).ToList();
    }

    public async Task<SkillTestAdminDto?> GetSkillTestByIdAsync(Guid id)
    {
        var test = await _context.PT_SkillTests.FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);
        return test == null ? null : ToAdminDto(test);
    }

    public async Task<SkillTestAdminDto?> UpdateSkillTestAsync(Guid id, UpdateSkillTestDto dto)
    {
        var test = await _context.PT_SkillTests.FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);
        if (test == null) return null;

        if (dto.Category != null) test.Category = dto.Category;
        if (dto.Difficulty.HasValue) test.Difficulty = dto.Difficulty.Value;
        if (dto.Title != null) test.Title = dto.Title;
        if (dto.Description != null) test.Description = dto.Description;
        if (dto.TimeLimit.HasValue) test.TimeLimit = dto.TimeLimit.Value;
        if (dto.Questions != null) test.Questions = JsonSerializer.Serialize(dto.Questions);
        if (dto.IsActive.HasValue) test.IsActive = dto.IsActive.Value;
        test.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return ToAdminDto(test);
    }

    public async Task<bool> DeleteSkillTestAsync(Guid id)
    {
        var test = await _context.PT_SkillTests.FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);
        if (test == null) return false;

        test.IsDeleted = true;
        test.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<SkillTestPublicDto>> GetAvailableTestsAsync(string? category)
    {
        var query = _context.PT_SkillTests.Where(t => !t.IsDeleted && t.IsActive);
        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(t => t.Category == category);

        var tests = await query.OrderBy(t => t.Category).ThenBy(t => t.Title).ToListAsync();
        return tests.Select(ToPublicDto).ToList();
    }

    public async Task<TestAttemptDto?> StartTestAsync(Guid candidateId, Guid testId)
    {
        var candidate = await _context.PT_Candidates.FirstOrDefaultAsync(c => c.Id == candidateId && !c.IsDeleted);
        if (candidate == null) throw new InvalidOperationException("Candidate not found");
        // Requiere perfil completo para iniciar (no para ver la lista) - fase-3-sub6.md pregunta 7.
        if (!candidate.WizardCompleted) throw new InvalidOperationException("Wizard not completed");

        var test = await _context.PT_SkillTests.FirstOrDefaultAsync(t => t.Id == testId && !t.IsDeleted && t.IsActive);
        if (test == null) return null;

        var existing = await _context.PT_CandidateTestResults
            .FirstOrDefaultAsync(r => r.PT_CandidateId == candidateId && r.PT_SkillTestId == testId && !r.IsDeleted);

        if (existing != null)
        {
            if (existing.CompletedAt == null && !HasExpired(existing, test))
            {
                // Intento en curso sin vencer - se devuelve el mismo en vez de crear otro (pregunta 8).
                return BuildAttemptDto(existing, test);
            }

            if (existing.CompletedAt == null && HasExpired(existing, test))
            {
                await CompleteAsTimeoutAsync(existing);
            }

            // 1 intento por reto, ya usado (completado o vencido) - fase-3-sub6.md pregunta 5.
            throw new InvalidOperationException("Attempt already used for this test");
        }

        var result = new PTCandidateTestResult
        {
            PT_CandidateId = candidateId,
            PT_SkillTestId = testId,
            StartedAt = DateTime.UtcNow
        };
        _context.PT_CandidateTestResults.Add(result);
        await _context.SaveChangesAsync();

        return BuildAttemptDto(result, test);
    }

    public async Task<TestResultDto?> SubmitTestAsync(Guid resultId, Guid candidateId, SubmitTestAnswersDto answers, int antiCheatFlags)
    {
        var result = await _context.PT_CandidateTestResults
            .Include(r => r.SkillTest)
            .FirstOrDefaultAsync(r => r.Id == resultId && r.PT_CandidateId == candidateId && !r.IsDeleted);
        if (result == null) return null;

        if (result.CompletedAt != null) return ToResultDto(result); // ya resuelto, idempotente.

        if (HasExpired(result, result.SkillTest))
        {
            await CompleteAsTimeoutAsync(result);
            await _scoringService.RecalculateAsync(candidateId);
            return ToResultDto(result);
        }

        var questions = DeserializeQuestions(result.SkillTest.Questions);
        var correct = 0;
        for (int i = 0; i < questions.Count; i++)
        {
            if (i < answers.Answers.Count && answers.Answers[i] == questions[i].CorrectIndex)
                correct++;
        }

        result.Score = questions.Count == 0 ? 0 : (int)Math.Round((double)correct / questions.Count * 100);
        result.TimeTaken = (int)(DateTime.UtcNow - result.StartedAt).TotalSeconds;
        result.CompletedAt = DateTime.UtcNow;
        result.AntiCheatFlags = antiCheatFlags;
        result.Answers = JsonSerializer.Serialize(answers.Answers);
        result.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Sexto componente de EvidenceIndex (fase-3-sub6.md pregunta 6).
        await _scoringService.RecalculateAsync(candidateId);

        return ToResultDto(result);
    }

    public async Task<List<TestResultDto>> GetTestResultsAsync(Guid candidateId)
    {
        var results = await _context.PT_CandidateTestResults
            .Include(r => r.SkillTest)
            .Where(r => r.PT_CandidateId == candidateId && !r.IsDeleted)
            .OrderByDescending(r => r.StartedAt)
            .ToListAsync();

        var anyExpired = false;
        foreach (var r in results.Where(r => r.CompletedAt == null && HasExpired(r, r.SkillTest)))
        {
            await CompleteAsTimeoutAsync(r);
            anyExpired = true;
        }
        if (anyExpired) await _scoringService.RecalculateAsync(candidateId);

        return results.Select(ToResultDto).ToList();
    }

    /// <summary>Tiene al menos 1 resultado completado con Score >= PassingScore (fase-3-sub6.md pregunta 6).</summary>
    public static async Task<bool> HasPassingResultAsync(AppDbContext context, Guid candidateId)
        => await context.PT_CandidateTestResults
            .AnyAsync(r => r.PT_CandidateId == candidateId && !r.IsDeleted && r.CompletedAt != null && r.Score >= PassingScore);

    private static bool HasExpired(PTCandidateTestResult result, PTSkillTest test)
        => DateTime.UtcNow > result.StartedAt.AddMinutes(test.TimeLimit);

    private async Task CompleteAsTimeoutAsync(PTCandidateTestResult result)
    {
        result.Score = 0;
        result.CompletedAt = DateTime.UtcNow;
        result.TimeTaken = (int)(result.CompletedAt.Value - result.StartedAt).TotalSeconds;
        result.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    private static TestAttemptDto BuildAttemptDto(PTCandidateTestResult result, PTSkillTest test)
    {
        var elapsed = (int)(DateTime.UtcNow - result.StartedAt).TotalSeconds;
        var remaining = Math.Max(0, test.TimeLimit * 60 - elapsed);

        return new TestAttemptDto
        {
            ResultId = result.Id,
            Test = ToPublicDto(test),
            StartedAt = result.StartedAt,
            SecondsRemaining = remaining
        };
    }

    private static List<SkillTestQuestionDto> DeserializeQuestions(string? json)
        => string.IsNullOrWhiteSpace(json) ? new() : JsonSerializer.Deserialize<List<SkillTestQuestionDto>>(json) ?? new();

    private static SkillTestAdminDto ToAdminDto(PTSkillTest t) => new()
    {
        Id = t.Id,
        Category = t.Category,
        Difficulty = t.Difficulty,
        Title = t.Title,
        Description = t.Description,
        TimeLimit = t.TimeLimit,
        Questions = DeserializeQuestions(t.Questions),
        IsActive = t.IsActive
    };

    private static SkillTestPublicDto ToPublicDto(PTSkillTest t) => new()
    {
        Id = t.Id,
        Category = t.Category,
        Difficulty = t.Difficulty,
        Title = t.Title,
        Description = t.Description,
        TimeLimit = t.TimeLimit,
        // Nunca se expone CorrectIndex al candidato.
        Questions = DeserializeQuestions(t.Questions).Select(q => new SkillTestQuestionOnlyDto { Question = q.Question, Options = q.Options }).ToList()
    };

    private static TestResultDto ToResultDto(PTCandidateTestResult r) => new()
    {
        Id = r.Id,
        SkillTestId = r.PT_SkillTestId,
        TestTitle = r.SkillTest?.Title ?? string.Empty,
        Category = r.SkillTest?.Category ?? string.Empty,
        Score = r.Score,
        TimeTaken = r.TimeTaken,
        CompletedAt = r.CompletedAt,
        AntiCheatFlags = r.AntiCheatFlags
    };
}
