

using CommunityToolkit.Mvvm.ComponentModel;

namespace ProcraStop.Modules.Habits.Presentation.Models
{
    public partial class DayItem : ObservableObject
    {
        public DayOfWeek Day { get; }

        public string Label => Day.ToString();

        [ObservableProperty]
        private bool isSelected;

        public DayItem(DayOfWeek day)
        {
            Day = day;
        }
    }
}
