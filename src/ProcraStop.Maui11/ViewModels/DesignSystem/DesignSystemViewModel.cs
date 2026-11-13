using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ProcraStop.Maui11.ViewModels.DesignSystem;

/// <summary>Fila del demo de chips (estado por chip, OneWay al control).</summary>
public partial class ChipDemoModel : ObservableObject
{
    [ObservableProperty]
    public partial string Label { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsSelected { get; set; }
}

/// <summary>
/// ViewModel de la galería del Design System (DesignSystemPage).
/// Solo demuestra el contrato de cada PS*: todos los valores ya vienen
/// "calculados" (por eso los componentes no contienen lógica).
/// </summary>
public partial class DesignSystemViewModel : ObservableObject
{
    // ===== Chips =====
    [ObservableProperty]
    public partial ObservableCollection<ChipDemoModel> Chips { get; set; } = [
        new() { Label = "Hoy", IsSelected = true },
        new() { Label = "Semana", IsSelected = false },
        new() { Label = "Mes", IsSelected = false },
        new() { Label = "Completadas", IsSelected = false },
    ];

    // ===== StatCard =====
    [ObservableProperty]
    public partial string FocusMinutes { get; set; } = "1240";

    [ObservableProperty]
    public partial string FocusDelta { get; set; } = "+12%";

    [ObservableProperty]
    public partial bool FocusDeltaPositive { get; set; } = true;

    // ===== ProgressCard =====
    [ObservableProperty]
    public partial double HabitsProgress { get; set; } = 0.65;

    [ObservableProperty]
    public partial string HabitsProgressText { get; set; } = "65%";

    // ===== Inputs =====
    [ObservableProperty]
    public partial string DemoName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool NameHasError { get; set; }

    [ObservableProperty]
    public partial string NameError { get; set; } = "Ingresa un nombre válido.";

    // ===== Demo de acciones =====
    [ObservableProperty]
    public partial string LastAction { get; set; } = "—";

    [RelayCommand]
    private void ToggleChip(ChipDemoModel? chip)
    {
        if (chip is not null)
            chip.IsSelected = !chip.IsSelected;
    }

    [RelayCommand]
    private void AddFocus()
    {
        var minutes = 1240 + Random.Shared.Next(5, 25);
        FocusMinutes = minutes.ToString("N0").Replace(",", ".", StringComparison.Ordinal);
        FocusDelta = "+8%";
        FocusDeltaPositive = true;
        LastAction = "StatCard actualizado";
    }

    [RelayCommand]
    private void CycleProgress()
    {
        // Demo: cicla 25% → 50% → 75% → 100% → 25%.
        var next = HabitsProgress >= 0.99 ? 0.25 : HabitsProgress + 0.25;
        HabitsProgress = next;
        HabitsProgressText = $"{(int)Math.Round(next * 100)}%";
        LastAction = "ProgressCard actualizado";
    }

    [RelayCommand]
    private void ToggleNameError() => NameHasError = !NameHasError;

    [RelayCommand]
    private void PrimaryAction() => LastAction = "Acción primaria ejecutada";

    [RelayCommand]
    private void SecondaryAction() => LastAction = "Acción secundaria ejecutada";

    [RelayCommand]
    private void DestructiveAction() => LastAction = "Acción destructiva ejecutada";

    [RelayCommand]
    private void CreateTask() => LastAction = "Acción del EmptyState";
}
