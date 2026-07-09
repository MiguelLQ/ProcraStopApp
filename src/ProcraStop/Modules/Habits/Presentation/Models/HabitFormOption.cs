using ProcraStop.Domain.Habits.Enums;

namespace ProcraStop.Modules.Habits.Presentation.Models;

public sealed record HabitUnitOption(string Label, HabitUnit Value);
public sealed record HabitTrackingOption(string Label, string Description, HabitTrackingType Value);
