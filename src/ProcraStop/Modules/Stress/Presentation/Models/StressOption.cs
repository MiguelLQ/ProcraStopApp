
using CommunityToolkit.Mvvm.ComponentModel;
using ProcraStop.Domain.Stress.Enums;

namespace ProcraStop.Modules.Stress.Presentation.Models
{
    public partial class StressOption : ObservableObject
    {
        public string Emoji { get; }
        public string Label { get; }
        public StressLevelInput Value { get; }

        [ObservableProperty]
        private bool isSelected;

        public StressOption(string emoji, string label, StressLevelInput value)
        {
            Emoji = emoji;
            Label = label;
            Value = value;
        }
    }
}
