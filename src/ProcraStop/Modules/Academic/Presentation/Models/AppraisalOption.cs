namespace ProcraStop.Modules.Academic.Presentation.Models
{
    // D1 EPA: opcion opcional para declarar interes/dificultad sin obligar al estudiante.
    public sealed class AppraisalOption
    {
        public int? Value { get; init; }
        public string Label { get; init; } = string.Empty;

        public static IReadOnlyList<AppraisalOption> All { get; } = new List<AppraisalOption>
        {
            new() { Value = null, Label = "Sin valorar" },
            new() { Value = 1, Label = "1 · Muy bajo" },
            new() { Value = 2, Label = "2 · Bajo" },
            new() { Value = 3, Label = "3 · Medio" },
            new() { Value = 4, Label = "4 · Alto" },
            new() { Value = 5, Label = "5 · Muy alto" }
        };

        public static AppraisalOption Find(int? value) =>
            All.FirstOrDefault(option => option.Value == value) ?? All[0];
    }
}
