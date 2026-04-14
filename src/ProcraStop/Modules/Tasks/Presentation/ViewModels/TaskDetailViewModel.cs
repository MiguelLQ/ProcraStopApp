using CommunityToolkit.Mvvm.ComponentModel;
using ProcraStop.Modules.Tasks.Application.Facade;
using ProcraStop.Modules.Tasks.Domain.Enums;
using ProcraStop.Modules.Tasks.Presentation.Models;
using System.Collections.ObjectModel;

namespace ProcraStop.Modules.Tasks.Presentation.ViewModels
{
    [QueryProperty(nameof(TaskIdRaw), "taskId")]
    public partial class TaskDetailViewModel : ObservableObject
    {
        private readonly ITasksFacade _tasksFacade;

        public TaskDetailViewModel(ITasksFacade tasksFacade)
        {
            _tasksFacade = tasksFacade;
            Console.WriteLine("TaskDetailViewModel creado");
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
        private TaskDetailModel? task;

        private async Task LoadAsync(Guid id)
        {
            Console.WriteLine($"LOAD DETAIL: {id}");

            var dto = await _tasksFacade.GetTaskByIdAsync(id);

            if (dto is null)
            {
                Console.WriteLine("DTO NULL");
                return;
            }

            Task = new TaskDetailModel
            {
                Id = dto.Id,
                Title = dto.Title,
                Description = dto.Description,
                DueDate = dto.DueDate,
                Priority = (TaskPriority)dto.Priority,
                Status = (TaskItemStatus)dto.Status,
                CourseName = dto.CourseName,
                Subtasks = new ObservableCollection<SubtaskModel>(
                    dto.Subtasks.Select(x => new SubtaskModel
                    {
                        Id = x.Id,
                        Title = x.Title,
                        IsCompleted = x.IsCompleted
                    }))
            };
        }

        // Explicit method to allow pages to pass the id without depending on generated properties
        public void SetTaskId(Guid id)
        {
            _ = LoadAsync(id);
        }
    }
}