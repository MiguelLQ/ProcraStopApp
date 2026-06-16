using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProcraStop.Application.Tasks.Queries.GetTasks;
using ProcraStop.Domain.Tasks.Enums;
using ProcraStop.Modules.Tasks.Presentation.Models;
using System.Collections.ObjectModel;

namespace ProcraStop.Modules.Tasks.Presentation.ViewModels
{
    public partial class TasksPageViewModel : ObservableObject
    {
        private readonly GetTasksQueryHandler _tasksQuery;

        private List<TaskItemModel> _allTasks = new();

        [ObservableProperty]
        private ObservableCollection<TaskItemModel> tasks = new();

        [ObservableProperty]
        private bool isBusy;


        [ObservableProperty]
        private TaskFilterOption selectedFilter = TaskFilterOption.All;

        public bool IsAllSelected => SelectedFilter == TaskFilterOption.All;
        public bool IsPendingSelected => SelectedFilter == TaskFilterOption.Pending;
        public bool IsCompletedSelected => SelectedFilter == TaskFilterOption.Completed;
        public bool IsOverdueSelected => SelectedFilter == TaskFilterOption.Overdue;

        public TasksPageViewModel(GetTasksQueryHandler tasksQuery)
        {
            _tasksQuery = tasksQuery;
        }

        partial void OnSelectedFilterChanged(TaskFilterOption value)
        {
            OnPropertyChanged(nameof(IsAllSelected));
            OnPropertyChanged(nameof(IsPendingSelected));
            OnPropertyChanged(nameof(IsCompletedSelected));
            OnPropertyChanged(nameof(IsOverdueSelected));
        }

        [RelayCommand]
        public async Task LoadAsync()
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;

                var result = await _tasksQuery.HandleAsync(
                    new GetTasksQuery { IncludeArchived = false },
                    CancellationToken.None);

                _allTasks = result
                    .Select(TaskItemModel.FromDto)
                    .ToList();

                ApplyFilter();
            }
            finally
            {
                IsBusy = false;
            }
        }
        [RelayCommand]
        private async Task OpenDetail(Guid taskId)
        {
            Console.WriteLine($"🔥 CLICK TASK: {taskId}");

            await Shell.Current.GoToAsync($"task-detail?taskId={taskId}");
        }
        private void ApplyFilter()
        {
            IEnumerable<TaskItemModel> filtered = SelectedFilter switch
            {
                TaskFilterOption.Pending => _allTasks.Where(x => x.Status != TaskItemStatus.Completed),
                TaskFilterOption.Completed => _allTasks.Where(x => x.Status == TaskItemStatus.Completed),
                TaskFilterOption.Overdue => _allTasks.Where(x => x.IsOverdue),
                _ => _allTasks
            };

            Tasks = new ObservableCollection<TaskItemModel>(filtered);
        }
        public ObservableCollection<TaskFilterChip> Filters { get; } = new()
        {
            new() { Title = "Todas", Filter = TaskFilterOption.All, IsSelected = true },
            new() { Title = "Pendientes", Filter = TaskFilterOption.Pending },
            new() { Title = "Completadas", Filter = TaskFilterOption.Completed },
            new() { Title = "Vencidas", Filter = TaskFilterOption.Overdue }
        };

        [RelayCommand]
        private void ChangeFilter(TaskFilterOption filter)
        {
            if (SelectedFilter == filter)
                return;

            SelectedFilter = filter;

            foreach (var item in Filters)
                item.IsSelected = item.Filter == filter;

            ApplyFilter(); // tu lógica actual
        }
    }
}