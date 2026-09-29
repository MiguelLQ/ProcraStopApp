using CommunityToolkit.Mvvm.ComponentModel;

namespace ProcraStop.Modules.Tasks.Presentation.Models
{
    public partial class SubtaskModel : ObservableObject
    {
        public Guid Id { get; set; }

        [ObservableProperty]
        private string title = string.Empty;

        [ObservableProperty]
        private bool isCompleted;

        [ObservableProperty]
        private bool hasPlannedDate;

        [ObservableProperty]
        private DateTime selectedPlannedDate = DateTime.Today;

        public DateTime? PlannedDate => HasPlannedDate ? SelectedPlannedDate.Date : null;
        public string PlanText => PlannedDate is DateTime date
            ? $"{Title} · {date:dd/MM}"
            : Title;

        partial void OnTitleChanged(string value) => OnPropertyChanged(nameof(PlanText));
        partial void OnHasPlannedDateChanged(bool value)
        {
            OnPropertyChanged(nameof(PlannedDate));
            OnPropertyChanged(nameof(PlanText));
        }
        partial void OnSelectedPlannedDateChanged(DateTime value)
        {
            OnPropertyChanged(nameof(PlannedDate));
            OnPropertyChanged(nameof(PlanText));
        }
    }
}
