using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProcraStop.Maui11.ViewModels.Common;

namespace ProcraStop.Maui11.ViewModels.Tasks;

/// <summary>
/// Etapa blank: aquí vivirán la colección de tareas, filtros (chips),
/// comandos de completar/detalle y navegación a create-task.
/// </summary>
public partial class TasksPageViewModel : ObservableObject
{
    /// <summary>Estado del Bottom Sheet "Agregar" (FAB +).</summary>
    public AddSheetViewModel AddSheet { get; }

    public TasksPageViewModel(AddSheetViewModel addSheet) => AddSheet = addSheet;

    /// <summary>Abre el Bottom Sheet "Agregar" (FAB +).</summary>
    [RelayCommand]
    private void OpenAddSheet() => AddSheet.IsOpen = true;
}
