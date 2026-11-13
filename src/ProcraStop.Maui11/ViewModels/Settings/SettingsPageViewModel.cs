using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProcraStop.Maui11.ViewModels.Common;

namespace ProcraStop.Maui11.ViewModels.Settings;

/// <summary>
/// Etapa blank: aquí vivirán tema (claro/oscuro/sistema), recordatorios y
/// gestión de datos.
/// </summary>
public partial class SettingsPageViewModel : ObservableObject
{
    /// <summary>Estado del Bottom Sheet "Agregar" (FAB +).</summary>
    public AddSheetViewModel AddSheet { get; }

    public SettingsPageViewModel(AddSheetViewModel addSheet) => AddSheet = addSheet;

    /// <summary>Abre el Bottom Sheet "Agregar" (FAB +).</summary>
    [RelayCommand]
    private void OpenAddSheet() => AddSheet.IsOpen = true;
}
