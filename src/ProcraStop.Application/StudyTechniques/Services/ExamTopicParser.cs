namespace ProcraStop.Application.StudyTechniques.Services;

// D2 EPA: los temas del examen se guardan como subtareas "Estudiar: X".
// Este parser evita que los pasos genericos (recordar/simulacro) cuenten como temas.
public static class ExamTopicParser
{
    private const string StudyPrefix = "Estudiar:";

    private static readonly string[] GenericPrefixes =
    {
        "Recordar los temas",
        "Simulacro breve"
    };

    public static IReadOnlyList<string> ExtractTopics(IEnumerable<string> subtaskTitles)
    {
        var titles = subtaskTitles
            .Select(title => title?.Trim() ?? string.Empty)
            .Where(title => title.Length > 0)
            .ToArray();

        var studyTopics = titles
            .Where(title => title.StartsWith(StudyPrefix, StringComparison.OrdinalIgnoreCase))
            .Select(title => title[StudyPrefix.Length..].Trim())
            .Where(title => title.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (studyTopics.Length > 0)
            return studyTopics;

        return titles
            .Where(title => !GenericPrefixes.Any(prefix =>
                title.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public static string NormalizeTopic(string? topic)
    {
        var value = topic?.Trim() ?? string.Empty;
        if (value.StartsWith(StudyPrefix, StringComparison.OrdinalIgnoreCase))
            value = value[StudyPrefix.Length..].Trim();

        return value;
    }
}
