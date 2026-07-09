
using CommunityToolkit.Mvvm.ComponentModel;

namespace ProcraStop.Modules.Tasks.Presentation.Models
{
    public partial class CalendarDayItem : ObservableObject
    {
        [ObservableProperty]
        private DateTime date;

        [ObservableProperty]
        private string dayName = string.Empty;

        [ObservableProperty]
        private string dayNumber = string.Empty;

        [ObservableProperty]
        private bool isCurrentMonth = true;

        [ObservableProperty]
        private bool isSelected;

        [ObservableProperty]
        private bool isToday; // editbycodex: distingue hoy sin alterar el estado seleccionado.

        [ObservableProperty]
        private bool hasTasks;

        [ObservableProperty]
        private bool hasOverdueTasks;

        [ObservableProperty]
        private int taskCount; // editbycodex: conteo disponible para accesibilidad y futuras etiquetas.
    }
}
