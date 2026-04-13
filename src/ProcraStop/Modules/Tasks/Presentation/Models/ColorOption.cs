

using CommunityToolkit.Mvvm.ComponentModel;

namespace ProcraStop.Modules.Tasks.Presentation.Models
{
    public partial class ColorOption : ObservableObject
    {
        public string Hex { get; set; } = string.Empty;

        [ObservableProperty]
        private bool isSelected;
    }
}
