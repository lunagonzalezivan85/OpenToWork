using OpenToWork.Shared.Challenges;

namespace OpenToWork.Tests;

/// <summary>
/// Retos de hosteleria: correccion, agregacion y validacion (logica pura, sin API ni BD).
/// </summary>
public class ChallengeScoringTests
{
    private static readonly Guid CompA = Guid.NewGuid();
    private static readonly Guid CompB = Guid.NewGuid();

    [Fact]
    public void SetScore_Parcial_RestaFalsosYNoBajaDeCero()
    {
        var correct = new[] { "a", "b" };
        Assert.Equal(1m, ChallengeScoring.SetScore(correct, new[] { "a", "b" }, MultipleChoiceScoring.Partial));
        Assert.Equal(0.5m, ChallengeScoring.SetScore(correct, new[] { "a" }, MultipleChoiceScoring.Partial));
        Assert.Equal(0m, ChallengeScoring.SetScore(correct, new[] { "a", "c" }, MultipleChoiceScoring.Partial));
        Assert.Equal(0m, ChallengeScoring.SetScore(correct, new[] { "c", "d", "e" }, MultipleChoiceScoring.Partial));
    }

    [Fact]
    public void SetScore_Exacta_TodoONada()
    {
        var correct = new[] { "a", "b" };
        Assert.Equal(1m, ChallengeScoring.SetScore(correct, new[] { "b", "a" }, MultipleChoiceScoring.Exact));
        Assert.Equal(0m, ChallengeScoring.SetScore(correct, new[] { "a" }, MultipleChoiceScoring.Exact));
    }

    [Fact]
    public void Ordenacion_PorRelaciones_YPorSecuencias()
    {
        var pairs = new ScoringRule
        {
            OrderingMode = OrderingScoring.PrecedencePairs,
            Precedences = { new() { Before = "a", After = "b" }, new() { Before = "a", After = "c" } }
        };
        Assert.Equal(1m, ChallengeScoring.OrderingScore(pairs, new[] { "a", "c", "b" })); // dos ordenes validos
        Assert.Equal(0.5m, ChallengeScoring.OrderingScore(pairs, new[] { "b", "a", "c" }));
        Assert.Equal(0m, ChallengeScoring.OrderingScore(pairs, Array.Empty<string>()));

        var seqs = new ScoringRule
        {
            OrderingMode = OrderingScoring.AcceptedSequences,
            AcceptedSequences = { new() { "a", "b", "c" }, new() { "a", "c", "b" } }
        };
        Assert.Equal(1m, ChallengeScoring.OrderingScore(seqs, new[] { "a", "c", "b" }));
        Assert.Equal(0m, ChallengeScoring.OrderingScore(seqs, new[] { "b", "a", "c" }));
    }

    [Theory]
    [InlineData("12,5", 12.5)]
    [InlineData("12.5", 12.5)]
    [InlineData(" 1.234,50 €", 1234.50)]
    [InlineData("-3", -3)]
    public void TryParseNumber_AceptaFormatosHabituales(string raw, double expected)
    {
        Assert.True(ChallengeScoring.TryParseNumber(raw, out var v));
        Assert.Equal((decimal)expected, v);
    }

    [Theory]
    [InlineData("")]
    [InlineData("doce")]
    [InlineData("=2+2")]
    public void TryParseNumber_RechazaTextoYFormulas(string raw) =>
        Assert.False(ChallengeScoring.TryParseNumber(raw, out _));

    [Fact]
    public void Numerico_ToleranciaAbsolutaYPorcentaje_ConPesos()
    {
        var fields = new List<NumericField>
        {
            new() { Key = "total", Expected = 100, Tolerance = 0.5m, Weight = 3 },
            new() { Key = "margen", Expected = 200, Tolerance = 1, ToleranceType = ToleranceType.Percent, Weight = 1 }
        };
        Assert.Equal(1m, ChallengeScoring.NumericScore(fields, new Dictionary<string, string> { ["total"] = "100,4", ["margen"] = "202" }));
        Assert.Equal(0.75m, ChallengeScoring.NumericScore(fields, new Dictionary<string, string> { ["total"] = "100", ["margen"] = "203" }));
        Assert.Equal(0m, ChallengeScoring.NumericScore(fields, new Dictionary<string, string>()));
    }

    [Fact]
    public void Agregado_PendientesNoCuentanComoCero_YCompletoSoloTrasRevisar()
    {
        var def = TwoActivityDefinition();
        var auto = new Dictionary<string, decimal?> { ["elegir"] = 1m, ["escribir"] = null };
        var answered = new HashSet<string> { "elegir", "escribir" };

        var pending = ChallengeScoring.Aggregate(def, auto, Array.Empty<CriterionScore>(), answered);
        Assert.Equal(100m, pending.AutoPercent);
        Assert.Null(pending.CompletePercent);
        Assert.Equal(1, pending.PendingReviewItems);
        var compB = pending.Competencies.Single(c => c.CompetencyId == CompB);
        Assert.Null(compB.Percent);
        Assert.False(compB.IsComplete);

        var done = ChallengeScoring.Aggregate(def, auto, new[] { new CriterionScore("escribir", "claridad", 2) }, answered);
        Assert.Equal(0, done.PendingReviewItems);
        Assert.Equal(75m, done.CompletePercent); // (1*1 + 0.5*1) / 2
        Assert.Equal(50m, done.Competencies.Single(c => c.CompetencyId == CompB).Percent);
    }

    [Fact]
    public void Agregado_ActividadAbiertaSinResponder_CuentaCeroYNoQuedaPendiente()
    {
        var def = TwoActivityDefinition();
        var r = ChallengeScoring.Aggregate(def, new Dictionary<string, decimal?> { ["elegir"] = 1m },
            Array.Empty<CriterionScore>(), new HashSet<string> { "elegir" });
        Assert.Equal(0, r.PendingReviewItems);
        Assert.Equal(50m, r.CompletePercent);
    }

    [Fact]
    public void Sanitize_DescartaClavesInventadas_RecortaTextoYSeleccionUnica()
    {
        var a = TwoActivityDefinition().Activities[0];
        var clean = ChallengeScoring.Sanitize(a, new ChallengeResponse { SelectedKeys = { "x", "a", "b" }, Text = "  hola  " });
        Assert.Equal(new[] { "a" }, clean.SelectedKeys);
        Assert.Equal("hola", clean.Text);

        var open = TwoActivityDefinition().Activities[1];
        var longText = ChallengeScoring.Sanitize(open, new ChallengeResponse { Text = new string('x', 5000) });
        Assert.Equal(open.MaxTextLength, longText.Text.Length);
    }

    [Fact]
    public void Validate_DefinicionCorrecta_NoTieneErrores() =>
        Assert.Empty(ChallengeRules.Validate(TwoActivityDefinition(), new[] { CompA, CompB }, jobTypeCount: 1));

    [Fact]
    public void Validate_DetectaPlantillaIncompatibleSinClaveYSinCargo()
    {
        var def = TwoActivityDefinition();
        def.Activities[0].Template = ChallengeTemplate.CalcSheet; // CalcSheet solo admite Numeric
        def.Activities[0].Scoring.OptionCredits.Clear();
        var errors = ChallengeRules.Validate(def, new[] { CompA, CompB }, jobTypeCount: 0);
        Assert.Contains(errors, e => e.Contains("no admite"));
        Assert.Contains(errors, e => e.Contains("ninguna opcion puntua"));
        Assert.Contains(errors, e => e.Contains("al menos a un cargo"));
    }

    [Fact]
    public void Validate_CompetenciaDesactivada_ImpidePublicar() =>
        Assert.Contains(ChallengeRules.Validate(TwoActivityDefinition(), new[] { CompA }, 1), e => e.Contains("competencia valida"));

    [Fact]
    public void VistaCandidato_NoLlevaClavesRubricasNiExplicaciones()
    {
        var def = TwoActivityDefinition();
        def.Activities[0].PracticeExplanation = "SECRETO-EXPLICACION";
        var json = ChallengeJson.Serialize(def.Activities.Select(ActivityViewDto.From).ToList());
        Assert.DoesNotContain("SECRETO-EXPLICACION", json);
        Assert.DoesNotContain("Credit", json);
        Assert.DoesNotContain("CorrectKeys", json);
        Assert.DoesNotContain("Rubric", json);
        Assert.DoesNotContain("Expected", json);
    }

    /// <summary>Seleccion unica (auto, CompA) + respuesta breve (rubrica, CompB).</summary>
    internal static ChallengeDefinition TwoActivityDefinition(Guid? compA = null, Guid? compB = null) => new()
    {
        Title = "Reto de prueba",
        Situation = "Turno de noche con mucha gente",
        Instructions = "Lee y responde",
        Deliverable = "Dos respuestas",
        AllowedTools = "Ninguna",
        AssessmentDescription = "Se valora la prioridad y la claridad",
        MaxEvaluationAttempts = 2,
        RetryCooldownDays = 7,
        Activities =
        {
            new ActivityDefinition
            {
                Key = "elegir", Title = "Que haces primero", Prompt = "Elige una",
                Template = ChallengeTemplate.Cards, ResponseType = ChallengeResponseType.SingleChoice,
                Options = { new() { Key = "a", Text = "Atender la alergia" }, new() { Key = "b", Text = "Cobrar la mesa 4" } },
                Scoring = new ScoringRule { OptionCredits = { new() { OptionKey = "a", Credit = 1 }, new() { OptionKey = "b", Credit = 0 } } },
                AutoCompetencies = { new() { CompetencyId = compA ?? CompA } },
                PracticeExplanation = "La seguridad va primero"
            },
            new ActivityDefinition
            {
                Key = "escribir", Title = "Responde al cliente", Prompt = "Escribe tu respuesta",
                Template = ChallengeTemplate.Conversation, ResponseType = ChallengeResponseType.ShortText,
                Conversation = { new() { Speaker = "Cliente", Text = "Llevamos 40 minutos esperando" } },
                AutoWeight = 0,
                MaxTextLength = 300,
                Rubric =
                {
                    new RubricCriterion
                    {
                        Key = "claridad", Title = "Claridad", CompetencyId = compB ?? CompB,
                        Levels = { [0] = "Nada", [1] = "Poco", [2] = "Algo", [3] = "Bien", [4] = "Excelente" }
                    }
                }
            }
        }
    };
}
