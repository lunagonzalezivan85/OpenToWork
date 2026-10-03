namespace OpenToWork.Shared.Challenges;

/// <summary>Respuesta del candidato a una actividad. Solo datos: el servidor la valida y la corrige.</summary>
public class ChallengeResponse
{
    /// <summary>Opciones elegidas (unica, multiple, acciones) o lineas marcadas (deteccion de errores).</summary>
    public List<string> SelectedKeys { get; set; } = new();
    /// <summary>Ordenacion: claves de las opciones en el orden elegido.</summary>
    public List<string> OrderedKeys { get; set; } = new();
    /// <summary>Calculo: valor por campo (texto tal cual lo escribio el candidato; admite coma decimal).</summary>
    public Dictionary<string, string> NumericValues { get; set; } = new();
    /// <summary>Respuesta breve o justificacion.</summary>
    public string Text { get; set; } = "";
}

/// <summary>Puntuacion de un criterio de rubrica por un revisor (0-4).</summary>
public record CriterionScore(string ActivityKey, string CriterionKey, int Score);

/// <summary>Resultado de una competencia dentro de un intento.</summary>
public class CompetencyResult
{
    public Guid CompetencyId { get; set; }
    /// <summary>0-100 sobre lo ya corregido. Null si todavia no hay nada corregido.</summary>
    public decimal? Percent { get; set; }
    /// <summary>False mientras falten criterios de revision humana de esta competencia.</summary>
    public bool IsComplete { get; set; }
}

public class ChallengeResult
{
    /// <summary>Resultado automatico parcial: solo la parte corregida automaticamente (0-100).</summary>
    public decimal? AutoPercent { get; set; }
    /// <summary>Resultado completo (0-100). Null mientras falten revisiones obligatorias.</summary>
    public decimal? CompletePercent { get; set; }
    /// <summary>Criterios de revision humana sin puntuar.</summary>
    public int PendingReviewItems { get; set; }
    public bool RequiresReview { get; set; }
    public List<CompetencyResult> Competencies { get; set; } = new();
}

/// <summary>
/// Correccion y agregacion. Formula (documentada en docs/dsiezar/retos-hosteleria.md):
///  1. Cada "elemento puntuable" se normaliza a 0-1: la parte automatica de una actividad
///     (segun su regla) y cada criterio de rubrica (puntuacion 0-4 / 4).
///  2. Cada elemento tiene un peso: AutoWeight * peso relativo de la competencia para la parte
///     automatica; el peso del criterio para la rubrica.
///  3. Competencia = suma(normalizado * peso) / suma(pesos) de sus elementos YA corregidos.
///  4. Los elementos pendientes NO cuentan como cero: se excluyen y la competencia queda incompleta.
///  5. Resultado completo = misma media ponderada sobre todos los elementos, solo si no falta ninguno.
/// Las notas de retos distintos no son comparables entre si (dificultad distinta).
/// </summary>
public static class ChallengeScoring
{
    /// <summary>Puntuacion 0-1 de la parte automatica; null si el tipo no tiene parte automatica.</summary>
    public static decimal? ScoreAuto(ActivityDefinition a, ChallengeResponse r)
    {
        var s = a.Scoring;
        switch (a.ResponseType)
        {
            case ChallengeResponseType.SingleChoice:
                return SingleCredit(s, r);
            case ChallengeResponseType.MultipleChoice:
            case ChallengeResponseType.ErrorDetection:
                return SetScore(s.CorrectKeys, r.SelectedKeys, s.MultipleMode);
            case ChallengeResponseType.ActionsWithJustification:
                return s.AllowMultipleSelection ? SetScore(s.CorrectKeys, r.SelectedKeys, s.MultipleMode) : SingleCredit(s, r);
            case ChallengeResponseType.Ordering:
                return OrderingScore(s, r.OrderedKeys);
            case ChallengeResponseType.Numeric:
                return NumericScore(s.NumericFields, r.NumericValues);
            default:
                return null; // ShortText: solo revision humana
        }
    }

    private static decimal SingleCredit(ScoringRule s, ChallengeResponse r)
    {
        if (r.SelectedKeys.Count != 1) return 0;
        return s.OptionCredits.FirstOrDefault(c => c.OptionKey == r.SelectedKeys[0])?.Credit ?? 0;
    }

    /// <summary>Exacta: 1 si coincide el conjunto. Parcial: (aciertos - falsos) / correctas, minimo 0.</summary>
    public static decimal SetScore(IList<string> correct, IList<string> selected, MultipleChoiceScoring mode)
    {
        if (correct.Count == 0) return 0;
        var sel = selected.Distinct().ToHashSet();
        var corr = correct.ToHashSet();
        if (mode == MultipleChoiceScoring.Exact) return sel.SetEquals(corr) ? 1 : 0;
        var hits = sel.Count(corr.Contains);
        var wrong = sel.Count - hits;
        return Math.Max(0, (decimal)(hits - wrong) / corr.Count);
    }

    public static decimal OrderingScore(ScoringRule s, IList<string> order)
    {
        if (order.Count == 0) return 0;
        if (s.OrderingMode == OrderingScoring.AcceptedSequences)
            return s.AcceptedSequences.Any(seq => seq.SequenceEqual(order)) ? 1 : 0;

        if (s.Precedences.Count == 0) return 0;
        var pos = order.Select((k, i) => (k, i)).GroupBy(x => x.k).ToDictionary(g => g.Key, g => g.First().i);
        var ok = s.Precedences.Count(p => pos.TryGetValue(p.Before, out var b) && pos.TryGetValue(p.After, out var a) && b < a);
        return (decimal)ok / s.Precedences.Count;
    }

    public static decimal NumericScore(IList<NumericField> fields, IDictionary<string, string> values)
    {
        if (fields.Count == 0) return 0;
        decimal total = 0, got = 0;
        foreach (var f in fields)
        {
            total += f.Weight;
            if (values.TryGetValue(f.Key, out var raw) && TryParseNumber(raw, out var v) && WithinTolerance(f, v))
                got += f.Weight;
        }
        return total == 0 ? 0 : got / total;
    }

    public static bool WithinTolerance(NumericField f, decimal value)
    {
        var allowed = f.ToleranceType == ToleranceType.Percent ? Math.Abs(f.Expected) * f.Tolerance / 100m : f.Tolerance;
        return Math.Abs(value - f.Expected) <= allowed;
    }

    /// <summary>Acepta "12,5", "12.5", "1.234,50" y " 12 ". No acepta formulas ni texto.</summary>
    public static bool TryParseNumber(string? raw, out decimal value)
    {
        value = 0;
        if (string.IsNullOrWhiteSpace(raw)) return false;
        var t = raw.Trim().Replace(" ", "").Replace("€", "").Replace("%", "");
        if (t.Contains(',') && t.Contains('.')) t = t.Replace(".", "").Replace(',', '.'); // 1.234,50
        else t = t.Replace(',', '.');
        return decimal.TryParse(t, System.Globalization.NumberStyles.AllowLeadingSign | System.Globalization.NumberStyles.AllowDecimalPoint,
            System.Globalization.CultureInfo.InvariantCulture, out value);
    }

    /// <summary>
    /// Agrega un intento. autoScores: parte automatica por actividad (solo las respondidas).
    /// reviews: criterios puntuados. Las actividades sin responder cuentan como 0 en la parte
    /// automatica (el candidato entrego sin responder); los criterios humanos sin puntuar quedan pendientes.
    /// </summary>
    public static ChallengeResult Aggregate(ChallengeDefinition d, IDictionary<string, decimal?> autoScores,
        IEnumerable<CriterionScore> reviews, ISet<string>? answeredKeys = null)
    {
        var reviewMap = reviews.ToDictionary(x => (x.ActivityKey, x.CriterionKey), x => x.Score);
        var items = new List<(Guid Competency, decimal Weight, decimal? Value, bool IsAuto)>();

        foreach (var a in d.Activities)
        {
            if (ChallengeRules.HasAutoPart(a.ResponseType))
            {
                autoScores.TryGetValue(a.Key, out var auto);
                var value = auto ?? 0; // entregada sin responder = 0 en lo automatico
                var compTotal = a.AutoCompetencies.Sum(c => c.Weight);
                foreach (var c in a.AutoCompetencies)
                    items.Add((c.CompetencyId, compTotal == 0 ? 0 : a.AutoWeight * c.Weight / compTotal, value, true));
            }
            if (ChallengeRules.HasHumanPart(a.ResponseType))
            {
                var answered = answeredKeys == null || answeredKeys.Contains(a.Key);
                foreach (var c in a.Rubric)
                {
                    // Sin respuesta no hay nada que revisar: cuenta como 0 y no queda pendiente.
                    decimal? v = !answered ? 0
                        : reviewMap.TryGetValue((a.Key, c.Key), out var sc) ? Math.Clamp(sc, 0, 4) / 4m : null;
                    items.Add((c.CompetencyId, c.Weight, v, false));
                }
            }
        }

        var result = new ChallengeResult
        {
            RequiresReview = items.Any(i => !i.IsAuto),
            PendingReviewItems = items.Count(i => !i.IsAuto && i.Value == null)
        };

        result.AutoPercent = WeightedPercent(items.Where(i => i.IsAuto));
        result.CompletePercent = result.PendingReviewItems == 0 ? WeightedPercent(items) : null;

        foreach (var g in items.GroupBy(i => i.Competency))
        {
            result.Competencies.Add(new CompetencyResult
            {
                CompetencyId = g.Key,
                Percent = WeightedPercent(g.Where(i => i.Value != null)),
                IsComplete = g.All(i => i.Value != null)
            });
        }
        return result;
    }

    private static decimal? WeightedPercent(IEnumerable<(Guid Competency, decimal Weight, decimal? Value, bool IsAuto)> items)
    {
        var scored = items.Where(i => i.Value != null && i.Weight > 0).ToList();
        var w = scored.Sum(i => i.Weight);
        if (w == 0) return null;
        return Math.Round(scored.Sum(i => i.Value!.Value * i.Weight) / w * 100, 1);
    }

    /// <summary>
    /// Limpia una respuesta antes de guardarla: solo claves que existen en la actividad, texto
    /// recortado a la longitud maxima, valores numericos solo para campos definidos.
    /// </summary>
    public static ChallengeResponse Sanitize(ActivityDefinition a, ChallengeResponse r)
    {
        var optionKeys = a.Options.Select(o => o.Key).ToHashSet();
        var allowedSelection = a.ResponseType == ChallengeResponseType.ErrorDetection
            ? a.Resources.FirstOrDefault(x => x.Key == a.Scoring.ErrorResourceKey)?.Rows.Select(x => x.Key).ToHashSet() ?? new HashSet<string>()
            : optionKeys;

        var clean = new ChallengeResponse
        {
            SelectedKeys = (r.SelectedKeys ?? new()).Where(allowedSelection.Contains).Distinct().ToList(),
            OrderedKeys = (r.OrderedKeys ?? new()).Where(optionKeys.Contains).Distinct().ToList(),
            NumericValues = (r.NumericValues ?? new())
                .Where(kv => a.Scoring.NumericFields.Any(f => f.Key == kv.Key))
                .ToDictionary(kv => kv.Key, kv => (kv.Value ?? "").Trim().Length > 30 ? (kv.Value ?? "").Trim()[..30] : (kv.Value ?? "").Trim()),
            Text = (r.Text ?? "").Trim()
        };
        if (clean.Text.Length > a.MaxTextLength) clean.Text = clean.Text[..a.MaxTextLength];

        var singleSelect = a.ResponseType == ChallengeResponseType.SingleChoice
            || (a.ResponseType == ChallengeResponseType.ActionsWithJustification && !a.Scoring.AllowMultipleSelection);
        if (singleSelect && clean.SelectedKeys.Count > 1) clean.SelectedKeys = clean.SelectedKeys.Take(1).ToList();
        return clean;
    }

    /// <summary>True si la respuesta tiene algo que corregir (para marcar la actividad como respondida).</summary>
    public static bool IsAnswered(ActivityDefinition a, ChallengeResponse r) => a.ResponseType switch
    {
        ChallengeResponseType.Ordering => r.OrderedKeys.Count == a.Options.Count,
        ChallengeResponseType.Numeric => r.NumericValues.Values.Any(v => !string.IsNullOrWhiteSpace(v)),
        ChallengeResponseType.ShortText => !string.IsNullOrWhiteSpace(r.Text),
        ChallengeResponseType.ActionsWithJustification => r.SelectedKeys.Count > 0 && !string.IsNullOrWhiteSpace(r.Text),
        ChallengeResponseType.ErrorDetection => true, // "no hay errores" es una respuesta valida
        _ => r.SelectedKeys.Count > 0
    };
}
