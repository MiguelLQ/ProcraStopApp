using ProcraStop.Application.Tasks.DTOs;

namespace ProcraStop.Application.Tasks.Services;

public static class ExamPlanBuilder
{
    public static IReadOnlyList<PlannedStepDto> Build(
        IEnumerable<string> topics,
        DateTime startDate,
        DateTime examDate)
    {
        var names = topics.Select(x => x.Trim()).Where(x => x.Length > 0).ToArray();
        if (names.Length == 0)
            throw new ArgumentException("Agrega al menos un tema para preparar el examen.", nameof(topics));
        if (startDate.Date > examDate.Date)
            throw new ArgumentException("El inicio debe ser anterior al examen.", nameof(startDate));

        var lastStudyDay = examDate.Date.AddDays(-1);
        if (lastStudyDay < startDate.Date)
            lastStudyDay = examDate.Date;

        var span = (lastStudyDay - startDate.Date).Days;
        var steps = new List<PlannedStepDto>();
        for (var i = 0; i < names.Length; i++)
        {
            var day = startDate.Date.AddDays(span * i / Math.Max(1, names.Length));
            steps.Add(new PlannedStepDto { Title = $"Estudiar: {names[i]}", PlannedDate = day });
        }

        steps.Add(new PlannedStepDto
        {
            Title = "Recordar los temas sin apuntes y corregir errores",
            PlannedDate = startDate.Date.AddDays(span / 2)
        });
        steps.Add(new PlannedStepDto
        {
            Title = "Simulacro breve y revision de errores",
            PlannedDate = lastStudyDay
        });
        return steps.OrderBy(x => x.PlannedDate).ToArray();
    }
}
