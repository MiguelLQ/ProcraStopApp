using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProcraStop.Maui11.ViewModels.Common;

namespace ProcraStop.Maui11.ViewModels.Dashboard;

public partial class DashboardViewModel : ObservableObject
{
    /// <summary>Estado del Bottom Sheet "Agregar" (FAB +).</summary>
    public AddSheetViewModel AddSheet { get; }

    /// <summary>Saludo contextual según la hora (get-only: no necesita ser
    /// observable; el binding compilado evita el fallback a reflexión).</summary>
    public string Greeting => DateTime.Now.Hour switch
    {
        < 12 => "Buenos días",
        < 19 => "Buenas tardes",
        _ => "Buenas noches"
    };

    public DashboardViewModel(AddSheetViewModel addSheet) => AddSheet = addSheet;

    /// <summary>Abre el Bottom Sheet "Agregar" (FAB +).</summary>
    [RelayCommand]
    private void OpenAddSheet() => AddSheet.IsOpen = true;
}
