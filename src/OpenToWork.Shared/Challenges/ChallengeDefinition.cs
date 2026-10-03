namespace OpenToWork.Shared.Challenges;

// Retos de hosteleria ("Demuestra tus habilidades"). Todo el contenido de un reto es DATOS: esta
// definicion se guarda como JSON en PT_Challenges.DraftJson (borrador editable) y se congela en
// PT_ChallengeVersions.ContentJson al publicar. Ver docs/dsiezar/retos-hosteleria.md.
//
// La plantilla visual (como se presenta) y el tipo de respuesta (como se corrige) van separados;
// ChallengeRules.CompatibleResponses dice que combinaciones se permiten.

/// <summary>Como se presenta la actividad. Nuevas plantillas (planos, cronogramas...) se anaden aqui
/// y en ChallengeRules.CompatibleResponses, sin tocar el motor de correccion.</summary>
public enum ChallengeTemplate
{
    Cards = 0,         // Tarjetas interactivas: elegir acciones, ordenar prioridades
    Document = 1,      // Documentos: cartas, tickets, fichas, presupuestos; marcar errores
    CalcSheet = 2,     // Fichas de calculo: importes, cantidades, porcentajes, rendimientos
    Conversation = 3   // Conversaciones simuladas predefinidas (sin IA generativa)
}

/// <summary>Como se responde y se corrige.</summary>
public enum ChallengeResponseType
{
    SingleChoice = 0,
    MultipleChoice = 1,
    Ordering = 2,
    ErrorDetection = 3,
    Numeric = 4,
    ShortText = 5,               // revision humana con rubrica
    ActionsWithJustification = 6 // seleccion corregida automaticamente + justificacion revisada por una persona
}

public enum MultipleChoiceScoring
{
    /// <summary>Todo o nada: puntua solo si la seleccion coincide exactamente.</summary>
    Exact = 0,
    /// <summary>Parcial: (aciertos - selecciones incorrectas) / correctas, minimo 0. Se avisa al candidato.</summary>
    Partial = 1
}

public enum OrderingScoring
{
    /// <summary>Puntua si la secuencia coincide con alguna de las secuencias aceptadas.</summary>
    AcceptedSequences = 0,
    /// <summary>Fraccion de relaciones "A antes que B" que se cumplen (admite varios ordenes validos).</summary>
    PrecedencePairs = 1
}

public enum ToleranceType
{
    Absolute = 0,
    Percent = 1
}

public enum ChallengeLevel
{
    Basic = 0,
    Intermediate = 1,
    Advanced = 2
}

public class ChallengeDefinition
{
    public string Title { get; set; } = "";
    /// <summary>Situacion que se resolvera (texto de la tarjeta).</summary>
    public string Situation { get; set; } = "";
    public ChallengeLevel Level { get; set; } = ChallengeLevel.Basic;
    public int EstimatedMinutes { get; set; } = 10;

    /// <summary>Instrucciones generales (antes de empezar).</summary>
    public string Instructions { get; set; } = "";
    /// <summary>Que debe entregar el candidato.</summary>
    public string Deliverable { get; set; } = "";
    /// <summary>Herramientas permitidas (calculadora, notas...).</summary>
    public string AllowedTools { get; set; } = "";
    /// <summary>Como se valoran las respuestas, sin revelar soluciones.</summary>
    public string AssessmentDescription { get; set; } = "";

    public bool AllowPractice { get; set; } = true;
    public bool AllowEvaluation { get; set; } = true;
    /// <summary>Evaluaciones entregadas permitidas por candidato (1-5).</summary>
    public int MaxEvaluationAttempts { get; set; } = 2;
    /// <summary>Dias de espera entre evaluaciones entregadas (0-90).</summary>
    public int RetryCooldownDays { get; set; } = 7;

    /// <summary>Textos editoriales por competencia para el resultado (fortalezas / a practicar).</summary>
    public List<CompetencyFeedback> CompetencyFeedback { get; set; } = new();

    public List<ActivityDefinition> Activities { get; set; } = new();
}

public class CompetencyFeedback
{
    public Guid CompetencyId { get; set; }
    /// <summary>Se muestra si el resultado de la competencia es >= 75 %.</summary>
    public string StrengthText { get; set; } = "";
    /// <summary>Se muestra si el resultado de la competencia es < 50 %.</summary>
    public string PracticeText { get; set; } = "";
}

public class ActivityDefinition
{
    /// <summary>Identificador estable dentro del reto (las respuestas se guardan por esta clave).</summary>
    public string Key { get; set; } = "";
    public string Title { get; set; } = "";
    /// <summary>Contexto breve: que esta pasando en el turno. Puede traer informacion nueva.</summary>
    public string Context { get; set; } = "";
    /// <summary>Consigna: la accion que se pide.</summary>
    public string Prompt { get; set; } = "";

    public ChallengeTemplate Template { get; set; }
    public ChallengeResponseType ResponseType { get; set; }

    public List<ResourceDefinition> Resources { get; set; } = new();
    /// <summary>Opciones, acciones, elementos a ordenar o mensajes de conversacion a elegir.</summary>
    public List<OptionDefinition> Options { get; set; } = new();
    /// <summary>Conversacion: mensajes del cliente/companero mostrados antes de responder.</summary>
    public List<ConversationLine> Conversation { get; set; } = new();

    public ScoringRule Scoring { get; set; } = new();

    /// <summary>Peso de la parte automatica (0 si la actividad es solo de revision humana).</summary>
    public decimal AutoWeight { get; set; } = 1;
    /// <summary>Competencias a las que suma la parte automatica, con su peso relativo.</summary>
    public List<CompetencyWeight> AutoCompetencies { get; set; } = new();

    /// <summary>Criterios de revision humana (respuesta breve y justificaciones).</summary>
    public List<RubricCriterion> Rubric { get; set; } = new();

    /// <summary>Explicacion mostrada en practica tras responder (nunca en evaluacion).</summary>
    public string PracticeExplanation { get; set; } = "";

    /// <summary>Longitud maxima del texto (respuesta breve / justificacion).</summary>
    public int MaxTextLength { get; set; } = 600;
}

public class ResourceDefinition
{
    public string Key { get; set; } = "";
    public string Title { get; set; } = "";
    /// <summary>Carta, Ticket, Comanda, Ficha tecnica, Presupuesto, Nota, Tabla...</summary>
    public string Kind { get; set; } = "";
    /// <summary>Texto libre (notas, mensajes, explicaciones de calculo).</summary>
    public string Text { get; set; } = "";
    public List<string> Columns { get; set; } = new();
    public List<ResourceRow> Rows { get; set; } = new();
    public string Footer { get; set; } = "";
}

public class ResourceRow
{
    /// <summary>Clave estable: en deteccion de errores se marca la fila por esta clave.</summary>
    public string Key { get; set; } = "";
    public List<string> Cells { get; set; } = new();
}

public class OptionDefinition
{
    public string Key { get; set; } = "";
    public string Text { get; set; } = "";
}

public class ConversationLine
{
    /// <summary>Quien habla: "Cliente", "Cocina", "Proveedor"...</summary>
    public string Speaker { get; set; } = "";
    public string Text { get; set; } = "";
}

public class CompetencyWeight
{
    public Guid CompetencyId { get; set; }
    public decimal Weight { get; set; } = 1;
}

public class ScoringRule
{
    // Seleccion unica / acciones: valor de cada opcion (0-1). Permite varias respuestas defendibles.
    public List<OptionCredit> OptionCredits { get; set; } = new();

    // Seleccion multiple / acciones multiples / deteccion de errores
    public List<string> CorrectKeys { get; set; } = new();
    public MultipleChoiceScoring MultipleMode { get; set; } = MultipleChoiceScoring.Partial;
    /// <summary>Acciones con justificacion: true = se eligen varias acciones (multiple); false = una.</summary>
    public bool AllowMultipleSelection { get; set; }

    // Deteccion de errores: recurso cuyas filas se pueden marcar
    public string ErrorResourceKey { get; set; } = "";

    // Ordenacion
    public OrderingScoring OrderingMode { get; set; } = OrderingScoring.PrecedencePairs;
    public List<List<string>> AcceptedSequences { get; set; } = new();
    public List<PrecedencePair> Precedences { get; set; } = new();

    // Calculo numerico
    public List<NumericField> NumericFields { get; set; } = new();
}

public class OptionCredit
{
    public string OptionKey { get; set; } = "";
    /// <summary>0 = no valida, 1 = completamente valida; valores intermedios para opciones defendibles.</summary>
    public decimal Credit { get; set; }
}

public class PrecedencePair
{
    public string Before { get; set; } = "";
    public string After { get; set; } = "";
}

public class NumericField
{
    public string Key { get; set; } = "";
    public string Label { get; set; } = "";
    /// <summary>EUR, %, ml, uds, kg...</summary>
    public string Unit { get; set; } = "";
    public decimal Expected { get; set; }
    public decimal Tolerance { get; set; }
    public ToleranceType ToleranceType { get; set; } = ToleranceType.Absolute;
    /// <summary>Decimales que se piden (informativo para el candidato).</summary>
    public int Decimals { get; set; } = 2;
    /// <summary>Peso dentro de la actividad.</summary>
    public decimal Weight { get; set; } = 1;
}

public class RubricCriterion
{
    public string Key { get; set; } = "";
    public string Title { get; set; } = "";
    public Guid CompetencyId { get; set; }
    public decimal Weight { get; set; } = 1;
    /// <summary>Descripciones especificas del ejercicio para los niveles 0, 1, 2, 3 y 4.</summary>
    public List<string> Levels { get; set; } = new() { "", "", "", "", "" };
}
