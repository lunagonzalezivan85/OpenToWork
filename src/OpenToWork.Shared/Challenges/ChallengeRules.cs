namespace OpenToWork.Shared.Challenges;

/// <summary>
/// Reglas estructurales de los retos: que plantillas admiten que tipos de respuesta y que necesita
/// cada tipo para poder publicarse. El servidor valida siempre antes de guardar/publicar; el editor
/// del admin usa lo mismo para avisar antes.
/// </summary>
public static class ChallengeRules
{
    public const int MinEvaluationAttempts = 1;
    public const int MaxEvaluationAttemptsLimit = 5;
    public const int MaxCooldownDays = 90;
    public const int RubricLevels = 5; // 0..4

    /// <summary>Combinaciones permitidas plantilla -> tipos de respuesta.</summary>
    public static readonly IReadOnlyDictionary<ChallengeTemplate, ChallengeResponseType[]> CompatibleResponses =
        new Dictionary<ChallengeTemplate, ChallengeResponseType[]>
        {
            [ChallengeTemplate.Cards] = new[]
            {
                ChallengeResponseType.SingleChoice, ChallengeResponseType.MultipleChoice,
                ChallengeResponseType.Ordering, ChallengeResponseType.ActionsWithJustification
            },
            [ChallengeTemplate.Document] = new[]
            {
                ChallengeResponseType.ErrorDetection, ChallengeResponseType.SingleChoice,
                ChallengeResponseType.MultipleChoice
            },
            [ChallengeTemplate.CalcSheet] = new[] { ChallengeResponseType.Numeric },
            [ChallengeTemplate.Conversation] = new[]
            {
                ChallengeResponseType.SingleChoice, ChallengeResponseType.ShortText,
                ChallengeResponseType.ActionsWithJustification
            }
        };

    public static bool IsCompatible(ChallengeTemplate template, ChallengeResponseType response) =>
        CompatibleResponses.TryGetValue(template, out var allowed) && allowed.Contains(response);

    /// <summary>Tipos con parte corregida automaticamente.</summary>
    public static bool HasAutoPart(ChallengeResponseType t) => t != ChallengeResponseType.ShortText;

    /// <summary>Tipos con parte de revision humana obligatoria.</summary>
    public static bool HasHumanPart(ChallengeResponseType t) =>
        t is ChallengeResponseType.ShortText or ChallengeResponseType.ActionsWithJustification;

    /// <summary>
    /// Errores que impiden publicar. knownCompetencyIds: competencias activas existentes.
    /// Devuelve mensajes concretos ("Actividad 2: ...") para que el editor los muestre.
    /// </summary>
    public static List<string> Validate(ChallengeDefinition d, ICollection<Guid> knownCompetencyIds, int jobTypeCount)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(d.Title)) errors.Add("Falta el titulo del reto.");
        if (string.IsNullOrWhiteSpace(d.Situation)) errors.Add("Falta la situacion que se resolvera.");
        if (string.IsNullOrWhiteSpace(d.Instructions)) errors.Add("Faltan las instrucciones.");
        if (string.IsNullOrWhiteSpace(d.Deliverable)) errors.Add("Falta indicar que debe entregar el candidato.");
        if (string.IsNullOrWhiteSpace(d.AllowedTools)) errors.Add("Falta indicar las herramientas permitidas.");
        if (string.IsNullOrWhiteSpace(d.AssessmentDescription)) errors.Add("Falta explicar como se valoran las respuestas.");
        if (d.EstimatedMinutes is < 1 or > 120) errors.Add("El tiempo estimado debe estar entre 1 y 120 minutos.");
        if (!d.AllowPractice && !d.AllowEvaluation) errors.Add("Activa al menos una modalidad (practica o evaluacion).");
        if (d.AllowEvaluation && (d.MaxEvaluationAttempts < MinEvaluationAttempts || d.MaxEvaluationAttempts > MaxEvaluationAttemptsLimit))
            errors.Add($"La politica de reintentos debe permitir entre {MinEvaluationAttempts} y {MaxEvaluationAttemptsLimit} evaluaciones.");
        if (d.RetryCooldownDays < 0 || d.RetryCooldownDays > MaxCooldownDays)
            errors.Add($"La espera entre evaluaciones debe estar entre 0 y {MaxCooldownDays} dias.");
        if (jobTypeCount == 0) errors.Add("Asigna el reto al menos a un cargo.");
        if (d.Activities.Count < 1) errors.Add("El reto necesita al menos una actividad.");

        var keys = new HashSet<string>();
        for (var i = 0; i < d.Activities.Count; i++)
        {
            var a = d.Activities[i];
            var p = $"Actividad {i + 1}";
            if (string.IsNullOrWhiteSpace(a.Key)) errors.Add($"{p}: falta la clave interna.");
            else if (!keys.Add(a.Key)) errors.Add($"{p}: la clave '{a.Key}' esta repetida.");
            if (string.IsNullOrWhiteSpace(a.Title)) errors.Add($"{p}: falta el titulo.");
            if (string.IsNullOrWhiteSpace(a.Prompt)) errors.Add($"{p}: falta la consigna.");
            if (!IsCompatible(a.Template, a.ResponseType))
                errors.Add($"{p}: la plantilla {a.Template} no admite respuestas de tipo {a.ResponseType}.");
            ValidateResources(a, p, errors);
            ValidateScoring(a, p, errors, knownCompetencyIds);
        }

        foreach (var f in d.CompetencyFeedback)
            if (!knownCompetencyIds.Contains(f.CompetencyId))
                errors.Add("Hay textos de resultado asociados a una competencia que no existe o esta desactivada.");

        return errors;
    }

    private static void ValidateResources(ActivityDefinition a, string p, List<string> errors)
    {
        var resourceKeys = new HashSet<string>();
        foreach (var r in a.Resources)
        {
            if (string.IsNullOrWhiteSpace(r.Key) || !resourceKeys.Add(r.Key)) errors.Add($"{p}: hay un recurso sin clave o con la clave repetida.");
            if (string.IsNullOrWhiteSpace(r.Title)) errors.Add($"{p}: hay un recurso sin titulo.");
            if (string.IsNullOrWhiteSpace(r.Text) && r.Rows.Count == 0) errors.Add($"{p}: el recurso '{r.Title}' esta vacio.");
            if (r.Rows.Any(row => r.Columns.Count > 0 && row.Cells.Count != r.Columns.Count))
                errors.Add($"{p}: en el recurso '{r.Title}' hay filas con un numero de celdas distinto al de columnas.");
        }
        if (a.Template is ChallengeTemplate.Document or ChallengeTemplate.CalcSheet && a.Resources.Count == 0)
            errors.Add($"{p}: esta plantilla necesita al menos un recurso (documento o datos del ejercicio).");
        if (a.Template == ChallengeTemplate.Conversation && a.Conversation.Count == 0)
            errors.Add($"{p}: la conversacion no tiene mensajes.");
    }

    private static void ValidateScoring(ActivityDefinition a, string p, List<string> errors, ICollection<Guid> competencies)
    {
        var s = a.Scoring;
        var optionKeys = a.Options.Select(o => o.Key).ToHashSet();
        var needsOptions = a.ResponseType is ChallengeResponseType.SingleChoice or ChallengeResponseType.MultipleChoice
            or ChallengeResponseType.Ordering or ChallengeResponseType.ActionsWithJustification;

        if (needsOptions)
        {
            if (a.Options.Count < 2) errors.Add($"{p}: necesita al menos dos opciones.");
            if (a.Options.Any(o => string.IsNullOrWhiteSpace(o.Key) || string.IsNullOrWhiteSpace(o.Text)))
                errors.Add($"{p}: hay opciones sin texto o sin clave.");
            if (optionKeys.Count != a.Options.Count) errors.Add($"{p}: hay opciones con la clave repetida.");
        }

        switch (a.ResponseType)
        {
            case ChallengeResponseType.SingleChoice:
            case ChallengeResponseType.ActionsWithJustification when !s.AllowMultipleSelection:
                if (!s.OptionCredits.Any(c => c.Credit > 0)) errors.Add($"{p}: falta la clave (ninguna opcion puntua).");
                if (s.OptionCredits.Any(c => c.Credit < 0 || c.Credit > 1)) errors.Add($"{p}: el valor de cada opcion debe estar entre 0 y 1.");
                if (s.OptionCredits.Any(c => !optionKeys.Contains(c.OptionKey))) errors.Add($"{p}: la clave apunta a una opcion que no existe.");
                break;
            case ChallengeResponseType.MultipleChoice:
            case ChallengeResponseType.ActionsWithJustification:
                if (s.CorrectKeys.Count == 0) errors.Add($"{p}: falta la clave (opciones correctas).");
                if (s.CorrectKeys.Any(k => !optionKeys.Contains(k))) errors.Add($"{p}: la clave apunta a una opcion que no existe.");
                break;
            case ChallengeResponseType.Ordering:
                if (s.OrderingMode == OrderingScoring.AcceptedSequences)
                {
                    if (s.AcceptedSequences.Count == 0) errors.Add($"{p}: falta al menos una secuencia aceptada.");
                    if (s.AcceptedSequences.Any(seq => seq.Count != optionKeys.Count || !seq.ToHashSet().SetEquals(optionKeys)))
                        errors.Add($"{p}: cada secuencia aceptada debe contener todos los elementos una vez.");
                }
                else
                {
                    if (s.Precedences.Count == 0) errors.Add($"{p}: faltan las relaciones de prioridad.");
                    if (s.Precedences.Any(x => !optionKeys.Contains(x.Before) || !optionKeys.Contains(x.After) || x.Before == x.After))
                        errors.Add($"{p}: hay relaciones de prioridad con elementos que no existen o repetidos.");
                }
                break;
            case ChallengeResponseType.ErrorDetection:
                var res = a.Resources.FirstOrDefault(r => r.Key == s.ErrorResourceKey);
                if (res == null) errors.Add($"{p}: indica en que documento se marcan los errores.");
                else
                {
                    var rowKeys = res.Rows.Select(r => r.Key).ToHashSet();
                    if (s.CorrectKeys.Count == 0) errors.Add($"{p}: falta la clave (lineas con error).");
                    if (s.CorrectKeys.Any(k => !rowKeys.Contains(k))) errors.Add($"{p}: la clave apunta a lineas que no existen en '{res.Title}'.");
                    if (res.Rows.Any(r => string.IsNullOrWhiteSpace(r.Key))) errors.Add($"{p}: todas las lineas de '{res.Title}' necesitan clave.");
                }
                break;
            case ChallengeResponseType.Numeric:
                if (s.NumericFields.Count == 0) errors.Add($"{p}: faltan los campos de calculo.");
                foreach (var f in s.NumericFields)
                {
                    if (string.IsNullOrWhiteSpace(f.Key) || string.IsNullOrWhiteSpace(f.Label)) errors.Add($"{p}: hay un campo de calculo sin clave o sin etiqueta.");
                    if (string.IsNullOrWhiteSpace(f.Unit)) errors.Add($"{p}: el campo '{f.Label}' no indica la unidad.");
                    if (f.Tolerance < 0 || (f.ToleranceType == ToleranceType.Percent && f.Tolerance > 50))
                        errors.Add($"{p}: la tolerancia de '{f.Label}' no es valida.");
                    if (f.Weight <= 0) errors.Add($"{p}: el peso de '{f.Label}' debe ser mayor que 0.");
                    if (f.Decimals is < 0 or > 4) errors.Add($"{p}: los decimales de '{f.Label}' deben estar entre 0 y 4.");
                }
                break;
        }

        if (HasAutoPart(a.ResponseType))
        {
            if (a.AutoWeight <= 0) errors.Add($"{p}: el peso de la parte automatica debe ser mayor que 0.");
            if (a.AutoCompetencies.Count == 0) errors.Add($"{p}: asocia la parte automatica al menos a una competencia.");
        }
        if (a.AutoCompetencies.Any(c => !competencies.Contains(c.CompetencyId))) errors.Add($"{p}: hay competencias que no existen o estan desactivadas.");
        if (a.AutoCompetencies.Any(c => c.Weight <= 0)) errors.Add($"{p}: los pesos por competencia deben ser mayores que 0.");

        if (HasHumanPart(a.ResponseType))
        {
            if (a.Rubric.Count == 0) errors.Add($"{p}: una actividad abierta necesita rubrica.");
            if (a.MaxTextLength is < 50 or > 3000) errors.Add($"{p}: la longitud maxima del texto debe estar entre 50 y 3000.");
        }
        var criterionKeys = new HashSet<string>();
        foreach (var c in a.Rubric)
        {
            if (string.IsNullOrWhiteSpace(c.Key) || !criterionKeys.Add(c.Key)) errors.Add($"{p}: hay criterios de rubrica sin clave o repetidos.");
            if (string.IsNullOrWhiteSpace(c.Title)) errors.Add($"{p}: hay un criterio de rubrica sin titulo.");
            if (!competencies.Contains(c.CompetencyId)) errors.Add($"{p}: el criterio '{c.Title}' no tiene una competencia valida.");
            if (c.Weight <= 0) errors.Add($"{p}: el peso del criterio '{c.Title}' debe ser mayor que 0.");
            if (c.Levels.Count != RubricLevels || c.Levels.Any(string.IsNullOrWhiteSpace))
                errors.Add($"{p}: el criterio '{c.Title}' necesita la descripcion de los niveles 0 a 4.");
        }
    }
}
