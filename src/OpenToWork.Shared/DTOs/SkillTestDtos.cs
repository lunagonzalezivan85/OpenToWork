namespace OpenToWork.Shared.DTOs;

public class SkillTestQuestionDto
{
    public string Question { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
    public int CorrectIndex { get; set; }
}

/// <summary>Uso admin (CRUD) - incluye CorrectIndex.</summary>
public class CreateSkillTestDto
{
    public string Category { get; set; } = string.Empty;
    public int Difficulty { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int TimeLimit { get; set; } = 15;
    public List<SkillTestQuestionDto> Questions { get; set; } = new();
    public bool IsActive { get; set; } = true;
}

public class UpdateSkillTestDto
{
    public string? Category { get; set; }
    public int? Difficulty { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public int? TimeLimit { get; set; }
    public List<SkillTestQuestionDto>? Questions { get; set; }
    public bool? IsActive { get; set; }
}

/// <summary>Uso admin - vista completa, incluye CorrectIndex.</summary>
public class SkillTestAdminDto
{
    public Guid Id { get; set; }
    public string Category { get; set; } = string.Empty;
    public int Difficulty { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int TimeLimit { get; set; }
    public List<SkillTestQuestionDto> Questions { get; set; } = new();
    public bool IsActive { get; set; }
}

/// <summary>
/// Uso candidato (GetAvailableTestsAsync/StartTestAsync) - NUNCA incluye CorrectIndex, para no
/// filtrar la respuesta correcta en la red (fase-3-sub6.md).
/// </summary>
public class SkillTestPublicDto
{
    public Guid Id { get; set; }
    public string Category { get; set; } = string.Empty;
    public int Difficulty { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int TimeLimit { get; set; }
    public List<SkillTestQuestionOnlyDto> Questions { get; set; } = new();
}

public class SkillTestQuestionOnlyDto
{
    public string Question { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
}

/// <summary>Resultado de StartTestAsync - incluye el intento (resultId) y el reto sin respuestas.</summary>
public class TestAttemptDto
{
    public Guid ResultId { get; set; }
    public SkillTestPublicDto Test { get; set; } = null!;
    public DateTime StartedAt { get; set; }
    public int SecondsRemaining { get; set; }
}

public class SubmitTestAnswersDto
{
    public List<int> Answers { get; set; } = new();
}

public class TestResultDto
{
    public Guid Id { get; set; }
    public Guid SkillTestId { get; set; }
    public string TestTitle { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int Score { get; set; }
    public int TimeTaken { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int AntiCheatFlags { get; set; }
}
