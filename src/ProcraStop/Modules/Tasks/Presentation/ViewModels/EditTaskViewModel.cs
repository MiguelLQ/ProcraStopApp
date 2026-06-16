
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProcraStop.Application.Tasks.Commands.UpdateTask;
using ProcraStop.Application.Tasks.DTOs;
using ProcraStop.Application.Tasks.Facade;
using ProcraStop.Application.Tasks.Queries.GetCourses;
using ProcraStop.Application.Tasks.Queries.GetTaskById;
using ProcraStop.Domain.Tasks.Enums;
using ProcraStop.Modules.Tasks.Presentation.Models;
using ProcraStop.Shared.Services;
using System.Collections.ObjectModel;

namespace ProcraStop.Modules.Tasks.Presentation.ViewModels
{
    [QueryProperty(nameof(TaskIdRaw), "taskId")]
    public partial class EditTaskViewModel : ObservableObject
    {
        private readonly UpdateTaskCommandHandler _tasksUpdate;
        private readonly GetCoursesQueryHandler _getCoursesQuery;
        private readonly GetTaskByIdQueryHandler getTaskByIdQuery;
        public EditTaskViewModel(
            UpdateTaskCommandHandler tasksUpdate,
            GetCoursesQueryHandler getCoursesQuery,
            GetTaskByIdQueryHandler getTaskByIdQuery)
        {
            _tasksUpdate = tasksUpdate;
            _getCoursesQuery = getCoursesQuery;
            this.getTaskByIdQuery = getTaskByIdQuery;
        }

        // =========================
        // ID
        // =========================
        public string TaskIdRaw
        {
            set
            {
                if (Guid.TryParse(value, out var id))
                {
                    TaskId = id;

                    // 🔥 CARGA AUTOMÁTICA
                    _ = LoadAsync(id);
                }
            }
        }

        [ObservableProperty]
        private Guid taskId;

        // =========================
        // CAMPOS (MISMO CREATE)
        // =========================
        [ObservableProperty]
        private DateTime selectedDueDate = DateTime.Today;

        [ObservableProperty]
        private TimeSpan selectedDueTime = new TimeSpan(18, 0, 0);
        [ObservableProperty]
        private string title = string.Empty;

        [ObservableProperty]
        private string? description;

        [ObservableProperty]
        private bool hasDueDate;

        [ObservableProperty]
        private string estimatedMinutesText = "";

        [ObservableProperty]
        private TaskPriority selectedPriority;
        // Exponemos la lista de prioridades para enlazarla a la UI
        public IReadOnlyList<TaskPriority> Priorities { get; } =
            Enum.GetValues<TaskPriority>();

        // =========================
        // CURSO
        // =========================
        [ObservableProperty]
        private Guid? selectedCourseId;

        [ObservableProperty]
        private CourseDto? selectedCourse;

        [ObservableProperty]
        private List<CourseDto> courses = new();

        partial void OnSelectedCourseChanged(CourseDto? value)
        {
            SelectedCourseId = value?.Id == Guid.Empty ? null : value?.Id;
        }
        // notificar cambios
        partial void OnSelectedDueDateChanged(DateTime value)
        {
            OnPropertyChanged(nameof(ComputedDueDate));
        }

        partial void OnSelectedDueTimeChanged(TimeSpan value)
        {
            OnPropertyChanged(nameof(ComputedDueDate));
        }
        // =========================
        // RECORDATORIO (MISMO CREATE)
        // =========================
        // Exponemos la lista de offsets de recordatorio para enlazarla a la UI
        private DateTime? CalculateReminderDate(DateTime? dueDate, ReminderOffset offset)
        {
            if (!dueDate.HasValue || offset == ReminderOffset.None)
                return null;

            return dueDate.Value.AddMinutes(-(int)offset);
        }
        private ReminderOffset CalculateOffset(DateTime? dueDate, DateTime? reminderDate)
        {
            if (!dueDate.HasValue || !reminderDate.HasValue)
                return ReminderOffset.None;

            var diff = dueDate.Value - reminderDate.Value;

            var minutes = (int)diff.TotalMinutes;

            return minutes switch
            {
                1 => ReminderOffset.OneMinuteBefore,
                5 => ReminderOffset.FiveMinutesBefore,
                30 => ReminderOffset.ThirtyMinutesBefore,
                60 => ReminderOffset.OneHourBefore,
                1440 => ReminderOffset.OneDayBefore,
                _ => ReminderOffset.None
            };
        }
        public IReadOnlyList<ReminderOffset> ReminderOffsets { get; } =
            Enum.GetValues<ReminderOffset>();

        [ObservableProperty]
        private int selectedReminderOffsetIndex;

        public ReminderOffset SelectedReminderOffset =>
            SelectedReminderOffsetIndex >= 0 &&
            SelectedReminderOffsetIndex < ReminderOffsets.Count
                ? ReminderOffsets[SelectedReminderOffsetIndex]
                : ReminderOffset.None;

        // =========================
        // SUBTAREAS (CORREGIDO)
        // =========================
        [ObservableProperty]
        private ObservableCollection<SubtaskModel> subtasks = new();

        [ObservableProperty]
        private string newSubtaskText = string.Empty;

        // =========================
        // COMPUTED
        // =========================
        partial void OnHasDueDateChanged(bool value)
        {
            if (value)
            {
                // 🔥 SOLO si estaba vacío o no inicializado
                if (SelectedDueDate == default)
                    SelectedDueDate = DateTime.Today;

                if (SelectedDueTime == default)
                    SelectedDueTime = new TimeSpan(18, 0, 0);
            }

            OnPropertyChanged(nameof(ComputedDueDate));
        }
        public DateTime? ComputedDueDate =>
        HasDueDate
            ? SelectedDueDate.Date + SelectedDueTime
            : null;
        public bool CanSave =>
            !IsSaving &&
            !string.IsNullOrWhiteSpace(Title);

        [ObservableProperty]
        private bool isSaving;

        partial void OnTitleChanged(string value)
        {
            SaveCommand.NotifyCanExecuteChanged();
        }

        partial void OnIsSavingChanged(bool value)
        {
            SaveCommand.NotifyCanExecuteChanged();
        }

        // =========================
        // LOAD
        // =========================
        public async Task LoadAsync(Guid id)
        {
            TaskId = id;

            var dto = await getTaskByIdQuery.HandleAsync(
                new() { TaskId = id },
                CancellationToken.None); if (dto == null)
                return;
            var list = (await _getCoursesQuery.HandleAsync(new GetCoursesQuery(), CancellationToken.None)).ToList();

            list.Insert(0, new CourseDto
            {
                Id = Guid.Empty,
                Name = "Sin curso"
            });

            Courses = list;

            // MAPEO (igual que create pero inverso)
            Title = dto.Title;
            Description = dto.Description;
            SelectedPriority = Enum.IsDefined(typeof(TaskPriority), dto.Priority)
                ? (TaskPriority)dto.Priority
                : TaskPriority.Medium;
            HasDueDate = dto.DueDate != null;

            if (dto.DueDate.HasValue)
            {
                SelectedDueDate = dto.DueDate.Value.Date;
                SelectedDueTime = dto.DueDate.Value.TimeOfDay;
            }
            else
            {
                // 🔥 evita inconsistencias visuales
                SelectedDueDate = DateTime.Today;
                SelectedDueTime = new TimeSpan(18, 0, 0);
            }
            if (dto.DueDate.HasValue)
            {
                SelectedDueDate = dto.DueDate.Value.Date;
                SelectedDueTime = dto.DueDate.Value.TimeOfDay;
            }
         
            EstimatedMinutesText = dto.EstimatedMinutes?.ToString() ?? "";

            SelectedCourse = Courses.FirstOrDefault(x => x.Id == dto.CourseId);

            var incomingOffset = dto.ReminderOffset;

            for (int i = 0; i < ReminderOffsets.Count; i++)
            {
                Console.WriteLine($"Index {i}: {ReminderOffsets[i]} = {(int)ReminderOffsets[i]}");
            }

            // 🔥 ASIGNACIÓN
            var offset = CalculateOffset(dto.DueDate, dto.ReminderDate);

            Console.WriteLine($"OFFSET CALCULADO: {offset}");

            SelectedReminderOffsetIndex = ReminderOffsets
                .ToList()
                .FindIndex(x => x == offset);

            Console.WriteLine($"FINAL INDEX: {SelectedReminderOffsetIndex}");
            // 🔥 SUBTASKS CORRECTO
            Subtasks = new ObservableCollection<SubtaskModel>(
                dto.Subtasks.Select(s => new SubtaskModel
                {
                    Id = s.Id,
                    Title = s.Title,
                    IsCompleted = s.IsCompleted
                }));
          
        }

        // =========================
        // SAVE (UPDATE)
        // =========================
        [RelayCommand(CanExecute = nameof(CanSave))]
        private async Task SaveAsync()
        {
            if (IsSaving)
                return;

            try
            {
                IsSaving = true;
                var mappedSubtasks = Subtasks
                .Where(s => !string.IsNullOrWhiteSpace(s.Title)) // evita basura
                .Select(s => new SubtaskDto
                {
                    Id = s.Id,
                    Title = s.Title.Trim(),
                    IsCompleted = s.IsCompleted
                })
                .ToList();
                int? estimatedMinutes = null;

                if (!string.IsNullOrWhiteSpace(EstimatedMinutesText) &&
                    int.TryParse(EstimatedMinutesText, out var minutes))
                {
                    estimatedMinutes = minutes;
                }
                var reminderDate = CalculateReminderDate(ComputedDueDate, SelectedReminderOffset);

                var command = new UpdateTaskCommand
                {
                    Id = TaskId,
                    Title = Title.Trim(),
                    Description = string.IsNullOrWhiteSpace(Description)
                        ? null
                        : Description.Trim(),
                    DueDate = ComputedDueDate,
                    ReminderDate = reminderDate,
                    Priority = SelectedPriority,
                    CourseId = SelectedCourseId,
                    EstimatedMinutes = estimatedMinutes,
                    Subtasks = mappedSubtasks,
                  
                };

                await _tasksUpdate.HandleAsync(command, CancellationToken.None);

                PendingNotificationService.Set("Tarea actualizada");
                // redirigir atras
                await Shell.Current.GoToAsync("..");
            }
            finally
            {
                IsSaving = false;
            }
        }

        // =========================
        // SUBTASKS COMMANDS
        // =========================
        [RelayCommand]
        private void AddSubtask()
        {
            if (string.IsNullOrWhiteSpace(NewSubtaskText))
                return;

            Subtasks.Add(new SubtaskModel
            {
                Id = Guid.Empty, // Id se generará en la capa de dominio al guardar
                Title = NewSubtaskText.Trim(),
                IsCompleted = false
            });

            NewSubtaskText = string.Empty;
        }

        [RelayCommand]
        private void RemoveSubtask(SubtaskModel subtask)
        {
            if (Subtasks.Contains(subtask))
                Subtasks.Remove(subtask);
        }
    }
}
