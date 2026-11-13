using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ProcraStop.Maui11.ViewModels.Common;

/// <summary>
/// Estado del Bottom Sheet "Agregar" (overlay embebido PSAddSheet).
/// Las acciones rápidas cierran el sheet; cuando existan las pantallas de
/// destino (create-task, create-habit, check-in, pomodoro) estas mismas
/// comandos navegarán a sus rutas.
/// </summary>
public partial class AddSheetViewModel : ObservableObject
{
    /// <summary>Abierto/cerrado. Lo observa el overlay PSAddSheet.
    /// Partial property (MVVMTK0045: WinUI/AOT exige partial property,
    /// y SIN inicializador — el inicializador da CS8050/CS9248).</summary>
    [ObservableProperty]
    public partial bool IsOpen { get; set; }

    /// <summary>Cierra el sheet (scrim, X o acción).</summary>
    [RelayCommand]
    private void CloseSheet() => IsOpen = false;

    [RelayCommand]
    private void AddTask()
    {
        // TODO(navegación): GoToAsync("create-task") cuando exista la página.
        CloseSheet();
    }

    [RelayCommand]
    private void AddHabit()
    {
        // TODO(navegación): GoToAsync("create-habit") cuando exista la página.
        CloseSheet();
    }

    [RelayCommand]
    private void CheckInStress()
    {
        // TODO(navegación): GoToAsync("stress-checkin") cuando exista la página.
        CloseSheet();
    }

    [RelayCommand]
    private void StartStudySession()
    {
        // TODO(navegación): GoToAsync("pomodoro") cuando exista la página.
        CloseSheet();
    }
}
