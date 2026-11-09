using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProcraStop.Application.Tasks.Commands.CreateTask;
using ProcraStop.Application.Academic.DTOs;
using ProcraStop.Application.Academic.Queries.GetCourses;
using ProcraStop.Application.Tasks.DTOs;
using ProcraStop.Domain.Tasks.Enums;
using ProcraStop.Shared.Services;
using ProcraStop.Modules.Tasks.Presentation.Models;
using System.Collections.ObjectModel;

namespace ProcraStop.Modules.Tasks.Presentation.ViewModels
{
    public partial class CreateTaskPageViewModel : ObservableObject
    {
        private readonly CreateTaskCommandHandler _createTaskHandler;
        private readonly GetCoursesQueryHandler _getCoursesHandler;
            // subtareas
            [ObservableProperty]
        private ObservableCollection<SubtaskModel> subtasks = new();

        [ObservableProperty]
        private AcademicActivityType selectedActivityType = AcademicActivityType.Task;

        public IReadOnlyList<AcademicActivityOption> ActivityTypes { get; } = AcademicActivityOption.All;
        [ObservableProperty]
        private AcademicActivityOption selectedActivityOption = AcademicActivityOption.All[0];
        partial void OnSelectedActivityOptionChanged(AcademicActivityOption value)
            => SelectedActivityType = value.Value;
        public bool IsExam => SelectedActivityType == AcademicActivityType.Exam;
        public bool IsDeliverable => SelectedActivityType == AcademicActivityType.Deliverable;

        [ObservableProperty]
        private bool hasPlannedStart;

        [ObservableProperty]
        private DateTime selectedPlannedStartDate = DateTime.Today;

        [ObservableProperty]
        private string? firstStep;

        [ObservableProperty]
        private bool hasPlannedReview;

        [ObservableProperty]
        private DateTime selectedPlannedReviewDate = DateTime.Today;

        [ObservableProperty]
        private bool newSubtaskHasDate;

        [ObservableProperty]
        private DateTime newSubtaskDate = DateTime.Today;

        partial void OnSelectedActivityTypeChanged(AcademicActivityType value)
        {
            OnPropertyChanged(nameof(IsExam));
            OnPropertyChanged(nameof(IsDeliverable));
            if (value != AcademicActivityType.Deliverable)
                HasPlannedReview = false;
            if (value == AcademicActivityType.Exam)
                HasDueDate = true;
        }
        
        [ObservableProperty]
        private string newSubtaskText = string.Empty;

        [ObservableProperty]
        private Guid? selectedCourseId;

        [ObservableProperty]
        private CourseDto? selectedCourse;

        [ObservableProperty]
        private List<CourseDto> courses = new();

        [ObservableProperty]
        private string title = string.Empty;

        [ObservableProperty]
        private string? description;

        [ObservableProperty]
        private bool hasDueDate = true;

        [ObservableProperty]
        private DateTime selectedDueDate = DateTime.Today.AddDays(1);

        [ObservableProperty]
        private TimeSpan selectedDueTime = new TimeSpan(18, 0, 0);

        [ObservableProperty]
        private string estimatedMinutesText = "30";

        [ObservableProperty]
        private TaskPriority selectedPriority = TaskPriority.Medium;

        [ObservableProperty]
        private bool isSaving;

        [ObservableProperty]
        private string? validationMessage;

        [ObservableProperty]
        private int selectedReminderOffsetIndex = 0;
        // Exponemos la lista de prioridades para enlazarla a la UI
        public IReadOnlyList<TaskPriority> Priorities { get; } =
            Enum.GetValues<TaskPriority>();

        // Exponemos la lista de offsets de recordatorio para enlazarla a la UI
        public IReadOnlyList<ReminderOffset> ReminderOffsets { get; } =
            Enum.GetValues<ReminderOffset>();
       // 
        public ReminderOffset SelectedReminderOffset =>
    SelectedReminderOffsetIndex >= 0 &&
    SelectedReminderOffsetIndex < ReminderOffsets.Count
        ? ReminderOffsets[SelectedReminderOffsetIndex]
        : ReminderOffset.None;
        // Método para obtener el texto descriptivo de un ReminderOffset
        public string GetReminderText(ReminderOffset offset) => offset switch
        {
            ReminderOffset.None => "Sin recordatorio",
            ReminderOffset.OneMinuteBefore => "1 minuto antes (prueba)",
            ReminderOffset.FiveMinutesBefore => "5 minutos antes",
            ReminderOffset.ThirtyMinutesBefore => "30 minutos antes",
            ReminderOffset.OneHourBefore => "1 hora antes",
            ReminderOffset.OneDayBefore => "1 día antes",
            _ => "Personalizado"
        };
        public string ReminderPreviewText
        {
            get
            {
                if (!ComputedDueDate.HasValue || SelectedReminderOffset == ReminderOffset.None)
                    return "Sin recordatorio";

                var reminder = ComputedDueDate.Value.AddMinutes(-(int)SelectedReminderOffset);

                if (reminder <= DateTime.Now)
                    return "⚠️ El recordatorio sería en el pasado";

                return $"Recordatorio: {reminder:dd/MM HH:mm}";
            }
        }
        partial void OnSelectedReminderOffsetIndexChanged(int value)
        {
            OnPropertyChanged(nameof(ReminderPreviewText));
        }
        // Cuando se cambia el curso seleccionado,
        // actualizamos el SelectedCourseId para que el comando SaveAsync lo use al crear la tarea.
        partial void OnSelectedCourseChanged(CourseDto? value)
        {
            SelectedCourseId = value?.Id == Guid.Empty ? null : value?.Id;
        }
        public event Func<Task>? TaskCreated;

        public CreateTaskPageViewModel(CreateTaskCommandHandler createTaskHandler, GetCoursesQueryHandler getCoursesHandler)
        {
            _createTaskHandler = createTaskHandler;
            _getCoursesHandler = getCoursesHandler;
        }

        public DateTime? ComputedDueDate =>
            HasDueDate
                ? SelectedDueDate.Date + SelectedDueTime
                : null;

        public string DueDatePreviewText
        {
            get
            {
                if (!ComputedDueDate.HasValue)
                    return "Sin fecha límite";

                var due = ComputedDueDate.Value;
                var today = DateTime.Today;
                var tomorrow = today.AddDays(1);

                if (due.Date == today)
                    return $"Vence hoy a las {due:HH:mm}";

                if (due.Date == tomorrow)
                    return $"Vence mañana a las {due:HH:mm}";

                if (due.Date <= today.AddDays(7))
                    return $"Vence esta semana · {due:ddd HH:mm}";

                return $"Vence el {due:dd/MM/yyyy HH:mm}";
            }
        }

        public bool CanSave =>
            !IsSaving &&
            !string.IsNullOrWhiteSpace(Title) &&
            IsEstimatedMinutesValid();

        partial void OnTitleChanged(string value)
        {
            SaveCommand.NotifyCanExecuteChanged();
            OnPropertyChanged(nameof(CanSave));
        }

        partial void OnEstimatedMinutesTextChanged(string value)
        {
            SaveCommand.NotifyCanExecuteChanged();
            OnPropertyChanged(nameof(CanSave));
        }

        partial void OnIsSavingChanged(bool value)
        {
            SaveCommand.NotifyCanExecuteChanged();
            OnPropertyChanged(nameof(CanSave));
        }

        partial void OnHasDueDateChanged(bool value)
        {
            OnPropertyChanged(nameof(ComputedDueDate));
            OnPropertyChanged(nameof(DueDatePreviewText));
        }

        partial void OnSelectedDueDateChanged(DateTime value)
        {
            OnPropertyChanged(nameof(ComputedDueDate));
            OnPropertyChanged(nameof(DueDatePreviewText));
            OnPropertyChanged(nameof(ReminderPreviewText));

        }

        partial void OnSelectedDueTimeChanged(TimeSpan value)
        {
            OnPropertyChanged(nameof(ComputedDueDate));
            OnPropertyChanged(nameof(DueDatePreviewText));
            OnPropertyChanged(nameof(ReminderPreviewText));
        }

        private bool IsEstimatedMinutesValid()
        {
            if (string.IsNullOrWhiteSpace(EstimatedMinutesText))
                return true;

            return int.TryParse(EstimatedMinutesText, out var minutes) && minutes > 0;
        }

        [RelayCommand(CanExecute = nameof(CanSave))]
        private async Task SaveAsync()
        {
            if (IsSaving)
                return;

            int? estimatedMinutes = null;

            if (!string.IsNullOrWhiteSpace(EstimatedMinutesText))
            {
                if (!int.TryParse(EstimatedMinutesText, out var minutes) || minutes <= 0)
                    return;

                estimatedMinutes = minutes;
            }

            try
            {
                IsSaving = true;
                ValidationMessage = null;
                if (IsExam && (!ComputedDueDate.HasValue || Subtasks.Count == 0))
                    throw new InvalidOperationException("El examen necesita fecha y al menos un tema.");

                var command = new CreateTaskCommand
                {
                    Title = Title.Trim(),
                    Description = string.IsNullOrWhiteSpace(Description)
                        ? null
                        : Description.Trim(),
                    DueDate = ComputedDueDate,
                    ReminderOffset = SelectedReminderOffset,
                    ReminderDate = null, // se ignora en backend
                    EstimatedMinutes = estimatedMinutes,
                    Priority = SelectedPriority,
                    CourseId = selectedCourseId,
                    ActivityType = SelectedActivityType,
                    PlannedStartDate = HasPlannedStart ? SelectedPlannedStartDate.Date : null,
                    FirstStep = FirstStep,
                    PlannedReviewDate = IsDeliverable && HasPlannedReview ? SelectedPlannedReviewDate.Date : null,
                    Subtasks = IsExam ? Subtasks.Select(x => x.Title).ToList() : null,
                    Milestones = IsExam ? null : Subtasks.Select(x => new PlannedStepDto
                    {
                        Title = x.Title,
                        PlannedDate = x.PlannedDate
                    }).ToList()
                };
                await _createTaskHandler.HandleAsync(command);
                Console.WriteLine($"OFFSET FINAL: {SelectedReminderOffset}");
                Console.WriteLine($"INDEX: {SelectedReminderOffsetIndex}");

                PendingNotificationService.Set("Tarea creada con exito!");
                if (TaskCreated is { } taskCreated)
                    await taskCreated.Invoke();

            }
            catch (ArgumentException ex)
            {
                ValidationMessage = ex.Message;
            }
            catch (InvalidOperationException ex)
            {
                ValidationMessage = ex.Message;
            }
            finally
            {
                IsSaving = false;
            }
        }
        // commands para subtareas
        [RelayCommand]
        private void AddSubtask()
        {
            if (string.IsNullOrWhiteSpace(NewSubtaskText))
                return;

            Subtasks.Add(new SubtaskModel
            {
                Title = NewSubtaskText.Trim(),
                HasPlannedDate = NewSubtaskHasDate,
                SelectedPlannedDate = NewSubtaskDate
            });

            NewSubtaskText = string.Empty;

        }
        [RelayCommand]
        private void RemoveSubtask(SubtaskModel subtask)
        {
            if (Subtasks.Contains(subtask))
                Subtasks.Remove(subtask);
        }
        public async Task LoadAsync()
        {
            var list = (await _getCoursesHandler.HandleAsync(
                new GetCoursesQuery()))
                .ToList();
            //Insertamos opción "Sin curso"
            list.Insert(0, new CourseDto
            {
                Id = Guid.Empty,
                Name = "Sin curso",
                Color = null
            });

            Courses = list;

            //default seleccionado
            SelectedCourse = Courses.FirstOrDefault();
        }
    }
}
