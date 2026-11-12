namespace ProcraStop.Domain.StudyTechniques.Entities;

// D2 EPA: un simulacro concreto de un examen, con puntaje y errores observados.
public sealed class ExamSimulation
{
    private ExamSimulation() { }

    private ExamSimulation(
        Guid id,
        Guid examTaskId,
        int score,
        string? errors,
        DateTime takenAtUtc)
    {
        Id = id;
        ExamTaskId = examTaskId;
        Score = score;
        Errors = errors;
        TakenAtUtc = takenAtUtc;
    }

    public Guid Id { get; private set; }
    public Guid ExamTaskId { get; private set; }
    public int Score { get; private set; }
    public string? Errors { get; private set; }
    public DateTime TakenAtUtc { get; private set; }

    public static ExamSimulation Create(
        Guid examTaskId,
        int score,
        string? errors)
    {
        ValidateScore(score);
        ValidateErrors(errors);

        return new ExamSimulation(
            Guid.NewGuid(),
            examTaskId,
            score,
            Normalize(errors),
            DateTime.UtcNow);
    }

    public static ExamSimulation Rehydrate(
        Guid id,
        Guid examTaskId,
        int score,
        string? errors,
        DateTime takenAtUtc)
        => new(id, examTaskId, score, errors, takenAtUtc);

    private static void ValidateScore(int score)
    {
        if (score is < 0 or > 100)
            throw new ArgumentOutOfRangeException(
                nameof(score), "El puntaje del simulacro debe estar entre 0 y 100.");
    }

    private static void ValidateErrors(string? errors)
    {
        if (errors?.Trim().Length > 1000)
            throw new ArgumentException(
                "Los errores del simulacro deben tener como maximo 1000 caracteres.",
                nameof(errors));
    }

    private static string? Normalize(string? errors)
        => string.IsNullOrWhiteSpace(errors) ? null : errors.Trim();
}
