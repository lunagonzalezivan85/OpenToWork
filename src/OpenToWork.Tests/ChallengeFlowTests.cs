using Microsoft.EntityFrameworkCore;
using OpenToWork.Core.Interfaces;
using OpenToWork.Core.Services;
using OpenToWork.Models.Context;
using OpenToWork.Models.Entities;
using OpenToWork.Shared.Challenges;
using OpenToWork.Shared.DTOs;

namespace OpenToWork.Tests;

/// <summary>
/// Retos de hosteleria: recorrido completo contra una BD en memoria (sin API ni MySQL).
/// Cada test usa su propia BD.
/// </summary>
public class ChallengeFlowTests
{
    private sealed class NoAudit : IAuditLogService
    {
        public Task LogAsync(Guid adminUserId, string action, string entityType, Guid? entityId, string? changesJson, string? ipAddress) => Task.CompletedTask;
        public Task<List<AuditLogDto>> GetLogsAsync(int page, int pageSize) => Task.FromResult(new List<AuditLogDto>());
    }

    private sealed class World
    {
        public AppDbContext Db = null!;
        public ChallengeAdminService Admin = null!;
        public ChallengeService Candidate = null!;
        public ChallengeReviewService Review = null!;
        public Guid AdminId = Guid.NewGuid();
        public Guid CandidateUserId = Guid.NewGuid();
        public Guid CandidateId;
        public Guid JobTypeId;
        public Guid CompA, CompB;
    }

    private static World NewWorld()
    {
        var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var audit = new NoAudit();
        var w = new World
        {
            Db = db,
            Admin = new ChallengeAdminService(db, audit),
            Candidate = new ChallengeService(db),
            Review = new ChallengeReviewService(db, audit)
        };
        var level = new PTJobLevel { Name = "Responsables y Cualificados", SortOrder = 1 };
        var type = new PTJobType { Name = "Camarero/a", PT_JobLevelId = level.Id, IsActive = true };
        var a = new PTCompetency { Name = "Priorizacion" };
        var b = new PTCompetency { Name = "Comunicacion" };
        var cand = new PTCandidate { SCUserId = w.CandidateUserId, FirstName = "Ana", LastName = "Prueba" };
        db.AddRange(level, type, a, b, cand);
        db.SaveChanges();
        w.JobTypeId = type.Id; w.CompA = a.Id; w.CompB = b.Id; w.CandidateId = cand.Id;
        return w;
    }

    private static async Task<Guid> PublishedChallengeAsync(World w)
    {
        var created = await w.Admin.CreateChallengeAsync(new SaveChallengeDto
        {
            JobTypeIds = { w.JobTypeId },
            Definition = ChallengeScoringTests.TwoActivityDefinition(w.CompA, w.CompB)
        }, w.AdminId);
        Assert.True(created.Success);
        var pub = await w.Admin.PublishAsync(created.Id!.Value, w.AdminId);
        Assert.True(pub.Success, string.Join(" | ", pub.Errors));
        return created.Id!.Value;
    }

    private static async Task<Guid> SubmittedEvaluationAsync(World w, Guid challengeId, bool answerOpen = true)
    {
        var start = await w.Candidate.StartAttemptAsync(w.CandidateUserId, challengeId, new StartAttemptDto { Mode = AttemptMode.Evaluation, AcceptConditions = true });
        Assert.Null(start.Error);
        var id = start.Value!.AttemptId;
        await w.Candidate.SaveAnswerAsync(w.CandidateUserId, id, "elegir", new ChallengeResponse { SelectedKeys = { "a" } });
        if (answerOpen)
            await w.Candidate.SaveAnswerAsync(w.CandidateUserId, id, "escribir", new ChallengeResponse { Text = "Disculpe, le traigo la cuenta ya" });
        Assert.Null((await w.Candidate.SubmitAsync(w.CandidateUserId, id)).Error);
        return id;
    }

    // ---------------- Contenido inicial ----------------

    [Fact]
    public async Task Seed_PublicaTodoElContenidoSinErrores_YEsIdempotente()
    {
        var w = NewWorld();
        var first = await w.Admin.SeedInitialContentAsync(w.AdminId);
        Assert.Empty(first.Errors);
        Assert.True(first.ChallengesCreated > 0);
        Assert.Equal(first.ChallengesCreated, first.ChallengesPublished);

        var second = await w.Admin.SeedInitialContentAsync(w.AdminId);
        Assert.Empty(second.Errors);
        Assert.Equal(0, second.ChallengesCreated);
        Assert.Equal(0, second.CompetenciesCreated);
        Assert.Equal(0, second.JobTypesCreated);
        Assert.Equal(first.ChallengesCreated, second.ChallengesSkipped);
    }

    [Fact]
    public async Task Seed_ElCandidatoNoRecibeOrdenacionesYaResueltas()
    {
        var w = NewWorld();
        await w.Admin.SeedInitialContentAsync(w.AdminId);
        var solved = (await w.Db.PT_ChallengeVersions.ToListAsync())
            .SelectMany(v => ChallengeJson.Deserialize<ChallengeDefinition>(v.ContentJson).Activities)
            .Where(a => a.ResponseType == ChallengeResponseType.Ordering
                        && ChallengeScoring.OrderingScore(a.Scoring, ActivityViewDto.From(a).Options.Select(o => o.Key).ToList()) == 1)
            .Select(a => a.Key).ToList();
        Assert.Empty(solved);
    }

    // ---------------- Publicacion y versiones ----------------

    [Fact]
    public async Task Borrador_NoApareceEnCatalogo_HastaPublicar()
    {
        var w = NewWorld();
        var created = await w.Admin.CreateChallengeAsync(new SaveChallengeDto
        {
            JobTypeIds = { w.JobTypeId },
            Definition = ChallengeScoringTests.TwoActivityDefinition(w.CompA, w.CompB)
        }, w.AdminId);

        Assert.Empty(await w.Candidate.GetCatalogAsync(w.CandidateUserId, w.JobTypeId));
        Assert.Equal("notFound", (await w.Candidate.GetIntroAsync(w.CandidateUserId, created.Id!.Value)).Error);

        await w.Admin.PublishAsync(created.Id!.Value, w.AdminId);
        Assert.Single(await w.Candidate.GetCatalogAsync(w.CandidateUserId, w.JobTypeId));
    }

    [Fact]
    public async Task EditarPublicado_NoCambiaIntentoEnCurso_YElNuevoUsaV2()
    {
        var w = NewWorld();
        var id = await PublishedChallengeAsync(w);
        var practice = (await w.Candidate.StartAttemptAsync(w.CandidateUserId, id, new StartAttemptDto { Mode = AttemptMode.Practice })).Value!;

        var def = ChallengeScoringTests.TwoActivityDefinition(w.CompA, w.CompB);
        def.Title = "Reto editado";
        await w.Admin.SaveDraftAsync(id, new SaveChallengeDto { JobTypeIds = { w.JobTypeId }, Definition = def }, w.AdminId);

        // Borrador guardado pero sin publicar: el candidato sigue viendo v1.
        Assert.Equal("Reto de prueba", (await w.Candidate.GetIntroAsync(w.CandidateUserId, id)).Value!.Title);

        Assert.Equal(2, (await w.Admin.PublishAsync(id, w.AdminId)).VersionNumber);
        var resumed = (await w.Candidate.GetAttemptAsync(w.CandidateUserId, practice.AttemptId)).Value!;
        Assert.Equal(1, resumed.VersionNumber);
        Assert.Equal("Reto de prueba", resumed.Title);

        var eval = (await w.Candidate.StartAttemptAsync(w.CandidateUserId, id, new StartAttemptDto { Mode = AttemptMode.Evaluation, AcceptConditions = true })).Value!;
        Assert.Equal(2, eval.VersionNumber);
    }

    [Fact]
    public async Task Publicar_SinCambios_SeRechaza_YEliminarSoloBorradores()
    {
        var w = NewWorld();
        var id = await PublishedChallengeAsync(w);
        Assert.False((await w.Admin.PublishAsync(id, w.AdminId)).Success);
        Assert.False((await w.Admin.DeleteAsync(id, w.AdminId)).Success);

        var copy = await w.Admin.DuplicateAsync(id, w.AdminId);
        Assert.True((await w.Admin.DeleteAsync(copy.Id!.Value, w.AdminId)).Success);
    }

    // ---------------- Intentos ----------------

    [Fact]
    public async Task Evaluacion_ExigeCondiciones_YReanudarDevuelveElMismoIntento()
    {
        var w = NewWorld();
        var id = await PublishedChallengeAsync(w);
        Assert.Equal("conditions", (await w.Candidate.StartAttemptAsync(w.CandidateUserId, id, new StartAttemptDto { Mode = AttemptMode.Evaluation })).Error);

        var a = (await w.Candidate.StartAttemptAsync(w.CandidateUserId, id, new StartAttemptDto { Mode = AttemptMode.Evaluation, AcceptConditions = true })).Value!;
        await w.Candidate.SaveAnswerAsync(w.CandidateUserId, a.AttemptId, "elegir", new ChallengeResponse { SelectedKeys = { "b" } });
        var b = (await w.Candidate.StartAttemptAsync(w.CandidateUserId, id, new StartAttemptDto { Mode = AttemptMode.Evaluation, AcceptConditions = true })).Value!;

        Assert.Equal(a.AttemptId, b.AttemptId);
        Assert.Equal(new[] { "b" }, b.Answers["elegir"].Response.SelectedKeys);
        Assert.Null(b.Answers["elegir"].Feedback); // en evaluacion no hay feedback
    }

    [Fact]
    public async Task GuardarDosVeces_NoDuplica_EntregarDosVecesEsIdempotente_YLuegoBloquea()
    {
        var w = NewWorld();
        var id = await PublishedChallengeAsync(w);
        var attempt = (await w.Candidate.StartAttemptAsync(w.CandidateUserId, id, new StartAttemptDto { Mode = AttemptMode.Evaluation, AcceptConditions = true })).Value!.AttemptId;

        await w.Candidate.SaveAnswerAsync(w.CandidateUserId, attempt, "elegir", new ChallengeResponse { SelectedKeys = { "b" } });
        await w.Candidate.SaveAnswerAsync(w.CandidateUserId, attempt, "elegir", new ChallengeResponse { SelectedKeys = { "a" } });
        Assert.Equal(1, await w.Db.PT_ChallengeAnswers.CountAsync(x => x.PT_ChallengeAttemptId == attempt));

        var r1 = (await w.Candidate.SubmitAsync(w.CandidateUserId, attempt)).Value!;
        var r2 = (await w.Candidate.SubmitAsync(w.CandidateUserId, attempt)).Value!;
        Assert.Equal(r1.SubmittedAt, r2.SubmittedAt);
        Assert.Equal(100m, r1.AutoPercent); // cuenta la ultima respuesta guardada

        Assert.Equal("locked", (await w.Candidate.SaveAnswerAsync(w.CandidateUserId, attempt, "elegir", new ChallengeResponse { SelectedKeys = { "b" } })).Error);
    }

    [Fact]
    public async Task IntentoAjeno_EsForbidden()
    {
        var w = NewWorld();
        var id = await PublishedChallengeAsync(w);
        var attempt = (await w.Candidate.StartAttemptAsync(w.CandidateUserId, id, new StartAttemptDto { Mode = AttemptMode.Practice })).Value!.AttemptId;

        var other = new PTCandidate { SCUserId = Guid.NewGuid(), FirstName = "Otro" };
        w.Db.Add(other);
        await w.Db.SaveChangesAsync();
        Assert.Equal("forbidden", (await w.Candidate.GetAttemptAsync(other.SCUserId, attempt)).Error);
        Assert.Equal("forbidden", (await w.Candidate.SaveAnswerAsync(other.SCUserId, attempt, "elegir", new())).Error);
    }

    [Fact]
    public async Task Reintentos_RespetanEsperaYLimite()
    {
        var w = NewWorld();
        var id = await PublishedChallengeAsync(w); // 2 evaluaciones, 7 dias de espera
        var first = await SubmittedEvaluationAsync(w, id, answerOpen: false);

        Assert.Equal("cooldown", (await w.Candidate.StartAttemptAsync(w.CandidateUserId, id, new StartAttemptDto { Mode = AttemptMode.Evaluation, AcceptConditions = true })).Error);

        var row = await w.Db.PT_ChallengeAttempts.FirstAsync(a => a.Id == first);
        row.SubmittedAt = DateTime.UtcNow.AddDays(-8);
        await w.Db.SaveChangesAsync();
        var second = await SubmittedEvaluationAsync(w, id, answerOpen: false);
        row = await w.Db.PT_ChallengeAttempts.FirstAsync(a => a.Id == second);
        row.SubmittedAt = DateTime.UtcNow.AddDays(-8);
        await w.Db.SaveChangesAsync();

        Assert.Equal("attempts", (await w.Candidate.StartAttemptAsync(w.CandidateUserId, id, new StartAttemptDto { Mode = AttemptMode.Evaluation, AcceptConditions = true })).Error);
        // La practica sigue disponible.
        Assert.Null((await w.Candidate.StartAttemptAsync(w.CandidateUserId, id, new StartAttemptDto { Mode = AttemptMode.Practice })).Error);
    }

    [Fact]
    public async Task Archivar_ImpideNuevosIntentos_PeroElEnCursoSeTermina()
    {
        var w = NewWorld();
        var id = await PublishedChallengeAsync(w);
        var practice = (await w.Candidate.StartAttemptAsync(w.CandidateUserId, id, new StartAttemptDto { Mode = AttemptMode.Practice })).Value!.AttemptId;
        await w.Admin.ArchiveAsync(id, w.AdminId);

        Assert.Empty(await w.Candidate.GetCatalogAsync(w.CandidateUserId, w.JobTypeId));
        Assert.Equal("archived", (await w.Candidate.StartAttemptAsync(w.CandidateUserId, id, new StartAttemptDto { Mode = AttemptMode.Evaluation, AcceptConditions = true })).Error);
        Assert.Equal(practice, (await w.Candidate.StartAttemptAsync(w.CandidateUserId, id, new StartAttemptDto { Mode = AttemptMode.Practice })).Value!.AttemptId);
        Assert.Null((await w.Candidate.SubmitAsync(w.CandidateUserId, practice)).Error);

        await w.Admin.RestoreAsync(id, w.AdminId);
        Assert.Single(await w.Candidate.GetCatalogAsync(w.CandidateUserId, w.JobTypeId));
    }

    [Fact]
    public async Task Practica_DaFeedback_YNuncaVaARevision()
    {
        var w = NewWorld();
        var id = await PublishedChallengeAsync(w);
        var attempt = (await w.Candidate.StartAttemptAsync(w.CandidateUserId, id, new StartAttemptDto { Mode = AttemptMode.Practice })).Value!.AttemptId;
        var saved = (await w.Candidate.SaveAnswerAsync(w.CandidateUserId, attempt, "elegir", new ChallengeResponse { SelectedKeys = { "a" } })).Value!;
        Assert.Equal(100m, saved.Feedback!.AutoPercent);
        Assert.Equal("La seguridad va primero", saved.Feedback.Explanation);

        await w.Candidate.SaveAnswerAsync(w.CandidateUserId, attempt, "escribir", new ChallengeResponse { Text = "Texto" });
        var result = (await w.Candidate.SubmitAsync(w.CandidateUserId, attempt)).Value!;
        Assert.Equal(ReviewStatus.NotRequired, result.ReviewStatus);
        Assert.Empty(await w.Review.GetQueueAsync(w.AdminId, canSeeAll: true, includeCompleted: true));
        Assert.Equal("notFound", (await w.Review.GetDetailAsync(w.AdminId, true, attempt)).Error);
    }

    // ---------------- Revision ----------------

    [Fact]
    public async Task Revision_ReclutadorSoloAsignados_CompletarExigeTodo_YCierraElResultado()
    {
        var w = NewWorld();
        var id = await PublishedChallengeAsync(w);
        var attempt = await SubmittedEvaluationAsync(w, id);

        var before = (await w.Candidate.GetResultAsync(w.CandidateUserId, attempt)).Value!;
        Assert.Equal(ReviewStatus.Pending, before.ReviewStatus);
        Assert.Null(before.CompletePercent);
        Assert.Equal(1, before.PendingReviewItems);

        var recruiter = Guid.NewGuid();
        Assert.Empty(await w.Review.GetQueueAsync(recruiter, canSeeAll: false, includeCompleted: false));
        Assert.Equal("forbidden", (await w.Review.GetDetailAsync(recruiter, false, attempt)).Error);

        w.Db.Add(new PTCandidateRecruitment { SCUserId = w.CandidateUserId, AssignedToUserId = recruiter });
        await w.Db.SaveChangesAsync();
        Assert.Single(await w.Review.GetQueueAsync(recruiter, false, false));

        Assert.Equal("incomplete", (await w.Review.SaveAsync(recruiter, false, attempt, new SaveReviewDto { Complete = true })).Error);
        Assert.Equal("score", (await w.Review.SaveAsync(recruiter, false, attempt, new SaveReviewDto
        {
            Scores = { new() { ActivityKey = "escribir", CriterionKey = "claridad", Score = 7 } }
        })).Error);

        var done = await w.Review.SaveAsync(recruiter, false, attempt, new SaveReviewDto
        {
            Complete = true, GeneralComment = "Bien",
            Scores = { new() { ActivityKey = "escribir", CriterionKey = "claridad", Score = 4 } }
        });
        Assert.Null(done.Error);
        Assert.Equal("alreadyCompleted", (await w.Review.SaveAsync(recruiter, false, attempt, new SaveReviewDto())).Error);

        var after = (await w.Candidate.GetResultAsync(w.CandidateUserId, attempt)).Value!;
        Assert.Equal(ReviewStatus.Completed, after.ReviewStatus);
        Assert.Equal(100m, after.CompletePercent);
        Assert.Equal("Bien", after.ReviewerComment);
    }

    [Fact]
    public async Task Empresa_SinEntrega_NoVeResultados()
    {
        var w = NewWorld();
        var id = await PublishedChallengeAsync(w);
        await SubmittedEvaluationAsync(w, id);
        Assert.Equal("forbidden", (await w.Candidate.GetResultsForCompanyAsync(Guid.NewGuid(), w.CandidateId)).Error);
    }
}
