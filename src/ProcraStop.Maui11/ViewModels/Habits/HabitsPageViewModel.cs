using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProcraStop.Maui11.ViewModels.Common;

namespace ProcraStop.Maui11.ViewModels.Habits;

/// <summary>
/// Etapa blank: aquí vivirán los hábitos del día, streaks, calendario
/// semanal y comandos de check/incremento.
/// </summary>
public partial class HabitsPageViewModel : ObservableObject
{
    /// <summary>Estado del Bottom Sheet "Agregar" (FAB +).</summary>
    public AddSheetViewModel AddSheet { get; }

    public HabitsPageViewModel(AddSheetViewModel addSheet) => AddSheet = addSheet;

    /// <summary>Abre el Bottom Sheet "Agregar" (FAB +).</summary>
    [RelayCommand]
    private void OpenAddSheet() => AddSheet.IsOpen = true;
}
