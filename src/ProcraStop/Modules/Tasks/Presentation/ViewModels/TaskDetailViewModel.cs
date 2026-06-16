using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProcraStop.Application.Tasks.Commands.ArchiveTask;
using ProcraStop.Application.Tasks.Commands.CompleteTask;
using ProcraStop.Application.Tasks.Commands.ToggleSubtask;
using ProcraStop.Application.Tasks.Queries.GetTaskById;
using ProcraStop.Domain.Tasks.Enums;
using ProcraStop.Modules.Tasks.Presentation.Models;
using ProcraStop.Shared.Services;
using ProcraStop.Shared.UI.Components;
using System.Collections.ObjectModel;

namespace ProcraStop.Modules.Tasks.Presentation.ViewModels
{
    [QueryProperty(nameof(TaskIdRaw), "taskId")]
    public partial class TaskDetailViewModel : ObservableObject
    {
        private readonly ToggleSubtaskCommandHandler _toggleSubtaskCommandHandler;
        private readonly GetTaskByIdQueryHandler _getTaskByIdQuery;
        private readonly CompleteTaskCommandHandler _completeTaskCommandHandler;
        private readonly ArchiveTaskCommandHandler _archiveTaskCommandHandler;
        private readonly ISnackbarService _snackbarService;
        public Guid CurrentTaskId => CurrentTask?.Id ?? Guid.Empty;
        public bool CanEdit => CurrentTask?.Status != TaskItemStatus.Completed;
        public TaskDetailViewModel(
            ToggleSubtaskCommandHandler toggleSubtaskCommandHandler,
            GetTaskByIdQueryHandler getTaskByIdQuery,
            CompleteTaskCommandHandler completeTaskCommandHandler,
            ArchiveTaskCommandHandler archiveTaskCommandHandler,
            ISnackbarService snackbarService)
        {
            _toggleSubtaskCommandHandler = toggleSubtaskCommandHandler;
            _getTaskByIdQuery = getTaskByIdQuery;
            _completeTaskCommandHandler = completeTaskCommandHandler;
            _archiveTaskCommandHandler = archiveTaskCommandHandler;
            _snackbarService = snackbarService;
        }
        [RelayCommand]
        private async Task ToggleSubtask(SubtaskModel subtask)
        {
            if (CurrentTask is null || subtask is null)
                return;

            Console.WriteLine($"[TaskDetailViewModel] ToggleSubtask invoked. TaskId={CurrentTask.Id}, SubtaskId={subtask.Id}");

            // Because the CheckBox two-way binding updates the subtask's IsCompleted
            // before the command runs, CurrentTask.Progress already reflects the new state.
            // Compute the previous progress by reversing this subtask's change.
            var total = CurrentTask.Subtasks.Count;
            var currentCompleted = CurrentTask.Subtasks.Count(s => s.IsCompleted); // after toggle
            var previousCompleted = subtask.IsCompleted ? currentCompleted - 1 : currentCompleted + 1;
            var previousProgress = total == 0 ? 0 : (double)previousCompleted / total;
            var wasCompleted = previousProgress >= 1;
            Console.WriteLine($"[TaskDetailViewModel] wasCompleted={wasCompleted}, previousProgress={previousProgress}, currentProgress={CurrentTask.Progress}");

            await _toggleSubtaskCommandHandler.HandleAsync(
                new ToggleSubtaskCommand
                {
                    TaskId = CurrentTask.Id,
                    SubtaskId = subtask.Id
                },
                CancellationToken.None);

            await LoadAsync(CurrentTask.Id);
            // esto es lo que te falta
            OnPropertyChanged(nameof(CurrentTask));

            var isNowCompleted = CurrentTask.Progress >= 1;
            Console.WriteLine($"[TaskDetailViewModel] isNowCompleted={isNowCompleted}, progress={CurrentTask.Progress}");

            if (!wasCompleted && isNowCompleted)
            {
                await _snackbarService.ShowAsync("¡La Tarea fue completada con éxito! Sigue Así 😉", SnackbarType.Success);
            }
        }
    
        [ObservableProperty]
        private string? taskIdRaw;

        partial void OnTaskIdRawChanged(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            if (Guid.TryParse(value, out var guid))
            {
                _ = LoadAsync(guid);
            }
            else
            {
                Console.WriteLine("GUID INVÁLIDO: " + value);
            }
        }

        [ObservableProperty]
        private TaskDetailModel? currentTask;

        private async Task LoadAsync(Guid id)
        {
            var dto = await _getTaskByIdQuery.HandleAsync(
                new GetTaskByIdQuery { TaskId = id },
                CancellationToken.None);

            if (dto is null)
                return;

            CurrentTask = new TaskDetailModel
            {
                Id = dto.Id,
                Title = dto.Title,
                Description = dto.Description,

                DueDate = dto.DueDate,
                ReminderDate = dto.ReminderDate,

                Priority = (TaskPriority)dto.Priority,
                Status = (TaskItemStatus)dto.Status,

                CourseName = dto.CourseName,
                CourseColor = dto.CourseColor ?? "#6B7280",

                Subtasks = new ObservableCollection<SubtaskModel>(
                    dto.Subtasks.Select(x => new SubtaskModel
                    {
                        Id = x.Id,
                        Title = x.Title,
                        IsCompleted = x.IsCompleted
                    }))
            };
            OnPropertyChanged(nameof(CanEdit));
        }
        // tareas sin subtareas
        [RelayCommand]
        private async Task CompleteTask()
        {
            if (CurrentTask is null)
                return;

            if (CurrentTask.Subtasks.Count > 0)
                return; // protección extra

            await _completeTaskCommandHandler.HandleAsync(
                new CompleteTaskCommand { TaskId = CurrentTask.Id },
                CancellationToken.None);

            await LoadAsync(CurrentTask.Id);

            await _snackbarService.ShowAsync("¡La Tarea fue completada! Sigue Así 😉", SnackbarType.Success);

        }
        [RelayCommand]
        private async Task DeleteTask()
        {
            if (CurrentTask is null) return;

            bool confirm = await Shell.Current.DisplayAlertAsync(
                "Eliminar tarea",
                "¿Estás seguro de eliminar esta tarea?",
                "Eliminar", "Cancelar");

            if (!confirm) return;

            await _archiveTaskCommandHandler.HandleAsync(
                new ArchiveTaskCommand { TaskId = CurrentTask.Id },
                CancellationToken.None);

            // Guarda el mensaje ANTES de navegar
            // Success (default)
            PendingNotificationService.Set("La tarea fue eliminada!", SnackbarType.Info);

            await Shell.Current.GoToAsync("..");
        }
        // Explicit method to allow pages to pass the id without depending on generated properties
        public void SetTaskId(Guid id)
        {
            _ = LoadAsync(id);
        }
        [RelayCommand]
        private async Task GoToEdit()
        {
            if (CurrentTask is null) return;

            await Shell.Current.GoToAsync($"edit-task?taskId={CurrentTask.Id}");
        }
    }
}