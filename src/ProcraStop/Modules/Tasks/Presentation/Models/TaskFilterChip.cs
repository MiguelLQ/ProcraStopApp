

using CommunityToolkit.Mvvm.ComponentModel;

namespace ProcraStop.Modules.Tasks.Presentation.Models
{
    public partial class TaskFilterChip : ObservableObject
    {
        [ObservableProperty]
        private string title = string.Empty;

        [ObservableProperty]
        private TaskFilterOption filter;

        [ObservableProperty]
        private bool isSelected;
    }
}
