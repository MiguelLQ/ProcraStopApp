using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProcraStop.Application.Academic.DTOs;
using ProcraStop.Application.Academic.Facade;
using ProcraStop.Application.Tasks.Commands.CreateTask;
using ProcraStop.Application.Tasks.Commands.UpdateTask;
using ProcraStop.Application.Tasks.DTOs;
using ProcraStop.Application.Tasks.Facade;
using ProcraStop.Domain.Tasks.Enums;
using ProcraStop.Maui11.Navigation;

namespace ProcraStop.Maui11.ViewModels.Tasks;

/// <summary>
/// Editor único de actividad académica. En creación aplica divulgación
/// progresiva; en edición carga y muestra todos los campos. Ambos caminos
/// reutilizan las fachadas y comandos de Application.
/// </summary>
public partial class TaskEditorViewModel : ObservableObject
{
    private readonly ITasksFacade _tasks;
    private readonly ICoursesFacade _courses;
    private bool _initialized;
    private Guid? _requestedTaskId;
    private AcademicActivityType _requestedActivityType = AcademicActivityType.Task;

    public ObservableCollection<CourseDto> Courses { get; } = [];
    public ObservableCollection<SubtaskDraft> Subtasks { get; } = [];

    public IReadOnlyList<TaskPriorityOption> PriorityOptions { get; } =
    [
        new(TaskPriority.Low, "Baja"),
        new(TaskPriority.Medium, "Media"),
        new(TaskPriority.High, "Alta")
    ];

    public IReadOnlyList<ReminderOption> ReminderOptions { get; } =
    [
        new(ReminderOffset.None, "Sin recordatorio"),
        new(ReminderOffset.FiveMinutesBefore, "5 minutos antes"),
        new(ReminderOffset.ThirtyMinutesBefore, "30 minutos antes"),
        new(ReminderOffset.OneHourBefore, "1 hora antes"),
        new(ReminderOffset.OneDayBefore, "1 día antes")
    ];

    [ObservableProperty] public partial string Title { get; set; }
    [ObservableProperty] public partial string? Description { get; set; }
    [ObservableProperty] public partial string? FirstStep { get; set; }
    [ObservableProperty] public partial string EstimatedMinutesText { get; set; }
    [ObservableProperty] public partial string NewSubtaskTitle { get; set; }
    [ObservableProperty] public partial string? TitleError { get; set; }
    [ObservableProperty] public partial string? ErrorMessage { get; set; }
    [ObservableProperty] public partial bool IsBusy { get; set; }
    [ObservableProperty] public partial bool IsEditMode { get; set; }
    [ObservableProperty] public partial bool ShowAdvanced { get; set; }
    [ObservableProperty] public partial bool HasDueDate { get; set; }
    [ObservableProperty] public partial bool HasPlannedStart { get; set; }
    [ObservableProperty] public partial bool HasPlannedReview { get; set; }
    [ObservableProperty] public partial DateTime DueDate { get; set; }
    [ObservableProperty] public partial TimeSpan DueTime { get; set; }
    [ObservableProperty] public partial DateTime PlannedStartDate { get; set; }
    [ObservableProperty] public partial DateTime PlannedReviewDate { get; set; }
    [ObservableProperty] public partial AcademicActivityType ActivityType { get; set; }
    [ObservableProperty] public partial CourseDto? SelectedCourse { get; set; }
    [ObservableProperty] public partial TaskPriorityOption SelectedPriority { get; set; }
    [ObservableProperty] public partial ReminderOption SelectedReminder { get; set; }

    public bool HasTitleError => !string.IsNullOrWhiteSpace(TitleError);
    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);
    public bool IsQuickMode => !IsEditMode && !ShowAdvanced;
    public bool IsAdvancedVisible => IsEditMode || ShowAdvanced;
    public bool IsExam => ActivityType == AcademicActivityType.Exam;
    public bool IsDeliverable => ActivityType == AcademicActivityType.Deliverable;
    public bool IsReading => ActivityType == AcademicActivityType.Reading;
    public bool ShowRequiredDueDate => IsExam || IsDeliverable;
    public bool ShowOptionalDueDate => !ShowRequiredDueDate;
    public bool ShowQuickEstimatedTime => IsReading;
    public bool ShowAdvancedEstimatedTime => !IsReading;
    public bool ShowReminderPicker => HasDueDate;
    public bool ShowOptionalDueEditors => ShowOptionalDueDate && HasDueDate;
    public bool ShowPlannedReviewDate => IsDeliverable && HasPlannedReview;
    public string SaveButtonText => IsEditMode ? "Guardar cambios" : "Crear actividad";

    public string PageTitle => IsEditMode ? "Editar actividad" : $"Nueva {ActivityName.ToLowerInvariant()}";
    public string HeaderTitle => IsEditMode ? "Todos los detalles" : $"Nueva {ActivityName.ToLowerInvariant()}";
    public string HeaderSubtitle => IsEditMode
        ? "Actualiza los campos que necesites."
        : "Completa lo esencial y guarda, o personaliza la planificación.";
    public string ActivityName => ActivityType switch
    {
        AcademicActivityType.Reading => "Lectura",
        AcademicActivityType.Exam => "Examen",
        AcademicActivityType.Deliverable => "Entregable",
        _ => "Tarea"
    };
    public string TitlePlaceholder => ActivityType switch
    {
        AcademicActivityType.Reading => "Ej. Capítulo 4 de biología",
        AcademicActivityType.Exam => "Ej. Examen parcial de cálculo",
        AcademicActivityType.Deliverable => "Ej. Informe del proyecto",
        _ => "Ej. Resolver práctica de álgebra"
    };
    public string SubtasksTitle => ActivityType switch
    {
        AcademicActivityType.Reading => "Capítulos o secciones",
        AcademicActivityType.Exam => "Temas del examen",
        AcademicActivityType.Deliverable => "Hitos del entregable",
        _ => "Subtareas"
    };
    public string SubtaskPlaceholder => IsReading ? "Ej. Capítulo 3" : "Añadir un paso";

    public TaskEditorViewModel(ITasksFacade tasks, ICoursesFacade courses)
    {
        _tasks = tasks;
        _courses = courses;
        Title = string.Empty;
        EstimatedMinutesText = string.Empty;
        NewSubtaskTitle = string.Empty;
        DueDate = DateTime.Today.AddDays(1);
        DueTime = new TimeSpan(18, 0, 0);
        PlannedStartDate = DateTime.Today;
        PlannedReviewDate = DateTime.Today;
        SelectedPriority = PriorityOptions[1];
        SelectedReminder = ReminderOptions[0];
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("taskId", out var taskIdValue) &&
            Guid.TryParse(taskIdValue?.ToString(), out var taskId))
        {
            _requestedTaskId = taskId;
            IsEditMode = true;
            ShowAdvanced = true;
        }

        if (query.TryGetValue("activityType", out var typeValue) &&
            Enum.TryParse<AcademicActivityType>(Uri.UnescapeDataString(typeValue?.ToString() ?? string.Empty), true, out var type))
            _requestedActivityType = type;
    }

    public async Task InitializeAsync()
    {
        if (_initialized)
            return;

        _initialized = true;
        IsBusy = true;
        ErrorMessage = null;
        try
        {
            Courses.Add(new CourseDto { Id = Guid.Empty, Name = "Sin curso" });
            try
            {
                var courses = await _courses.GetCoursesAsync();
                foreach (var course in courses)
                    Courses.Add(course);
            }
            catch (Exception exception)
            {
                // El curso es opcional: una falla de catálogo no debe impedir
                // crear una actividad rápida.
                System.Diagnostics.Trace.WriteLine($"[PS-TASK-EDITOR] courses: {exception}");
            }
            SelectedCourse = Courses[0];

            if (_requestedTaskId.HasValue)
                await LoadTaskAsync(_requestedTaskId.Value);
            else
                ConfigureForCreation(_requestedActivityType);
        }
        catch (Exception exception)
        {
            ErrorMessage = "No se pudo preparar el formulario. Inténtalo nuevamente.";
            System.Diagnostics.Trace.WriteLine($"[PS-TASK-EDITOR] init: {exception}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ConfigureForCreation(AcademicActivityType type)
    {
        ActivityType = type;
        HasDueDate = type is AcademicActivityType.Exam or AcademicActivityType.Deliverable;
        NotifyActivityPresentationChanged();
    }

    private async Task LoadTaskAsync(Guid taskId)
    {
        var task = await _tasks.GetTaskByIdAsync(taskId);
        if (task is null)
        {
            ErrorMessage = "La actividad ya no existe.";
            return;
        }

        ActivityType = task.ActivityType;
        Title = task.Title;
        Description = task.Description;
        FirstStep = task.FirstStep;
        EstimatedMinutesText = task.EstimatedMinutes?.ToString() ?? string.Empty;
        HasDueDate = task.DueDate.HasValue;
        DueDate = task.DueDate?.Date ?? DateTime.Today.AddDays(1);
        DueTime = task.DueDate?.TimeOfDay ?? new TimeSpan(18, 0, 0);
        HasPlannedStart = task.PlannedStartDate.HasValue;
        PlannedStartDate = task.PlannedStartDate?.Date ?? DateTime.Today;
        HasPlannedReview = task.PlannedReviewDate.HasValue;
        PlannedReviewDate = task.PlannedReviewDate?.Date ?? DateTime.Today;
        SelectedCourse = Courses.FirstOrDefault(x => x.Id == task.CourseId) ?? Courses[0];
        SelectedPriority = PriorityOptions.FirstOrDefault(x => (int)x.Value == task.Priority) ?? PriorityOptions[1];
        SelectedReminder = ReminderOptions.FirstOrDefault(x => x.Value == task.ReminderOffset) ?? ReminderOptions[0];
        foreach (var subtask in task.Subtasks)
            Subtasks.Add(new SubtaskDraft(subtask.Id, subtask.Title, subtask.IsCompleted, subtask.PlannedDate));
        NotifyActivityPresentationChanged();
    }

    partial void OnTitleChanged(string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            TitleError = null;
    }

    partial void OnTitleErrorChanged(string? value) => OnPropertyChanged(nameof(HasTitleError));
    partial void OnErrorMessageChanged(string? value) => OnPropertyChanged(nameof(HasError));
    partial void OnShowAdvancedChanged(bool value)
    {
        OnPropertyChanged(nameof(IsQuickMode));
        OnPropertyChanged(nameof(IsAdvancedVisible));
    }
    partial void OnIsEditModeChanged(bool value) => OnPropertyChanged(nameof(SaveButtonText));
    partial void OnHasDueDateChanged(bool value)
    {
        OnPropertyChanged(nameof(ShowReminderPicker));
        OnPropertyChanged(nameof(ShowOptionalDueEditors));
    }
    partial void OnHasPlannedReviewChanged(bool value) => OnPropertyChanged(nameof(ShowPlannedReviewDate));

    private void NotifyActivityPresentationChanged()
    {
        OnPropertyChanged(nameof(PageTitle));
        OnPropertyChanged(nameof(HeaderTitle));
        OnPropertyChanged(nameof(HeaderSubtitle));
        OnPropertyChanged(nameof(ActivityName));
        OnPropertyChanged(nameof(TitlePlaceholder));
        OnPropertyChanged(nameof(SubtasksTitle));
        OnPropertyChanged(nameof(SubtaskPlaceholder));
        OnPropertyChanged(nameof(IsExam));
        OnPropertyChanged(nameof(IsDeliverable));
        OnPropertyChanged(nameof(IsReading));
        OnPropertyChanged(nameof(ShowRequiredDueDate));
        OnPropertyChanged(nameof(ShowOptionalDueDate));
        OnPropertyChanged(nameof(ShowQuickEstimatedTime));
        OnPropertyChanged(nameof(ShowAdvancedEstimatedTime));
        OnPropertyChanged(nameof(ShowOptionalDueEditors));
        OnPropertyChanged(nameof(ShowPlannedReviewDate));
    }

    [RelayCommand]
    private void OpenAdvanced() => ShowAdvanced = true;

    [RelayCommand]
    private void AddSubtask()
    {
        if (string.IsNullOrWhiteSpace(NewSubtaskTitle))
            return;

        Subtasks.Add(new SubtaskDraft(Guid.Empty, NewSubtaskTitle.Trim(), false, null));
        NewSubtaskTitle = string.Empty;
    }

    [RelayCommand]
    private void RemoveSubtask(SubtaskDraft? subtask)
    {
        if (subtask is not null)
            Subtasks.Remove(subtask);
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (IsBusy || !Validate(out var estimatedMinutes))
            return;

        IsBusy = true;
        ErrorMessage = null;
        try
        {
            DateTime? dueDate = HasDueDate ? DueDate.Date + DueTime : null;
            var reminder = HasDueDate ? SelectedReminder.Value : ReminderOffset.None;
            Guid? courseId = SelectedCourse?.Id is { } id && id != Guid.Empty ? id : null;
            var subtaskTitles = Subtasks.Where(x => !string.IsNullOrWhiteSpace(x.Title)).Select(x => x.Title.Trim()).ToList();

            if (IsEditMode && _requestedTaskId.HasValue)
            {
                var result = await _tasks.UpdateTaskAsync(_requestedTaskId.Value, new UpdateTaskCommand
                {
                    Id = _requestedTaskId.Value,
                    Title = Title.Trim(),
                    Description = Clean(Description),
                    DueDate = dueDate,
                    ReminderDate = reminder == ReminderOffset.None ? null : dueDate?.AddMinutes(-(int)reminder),
                    ReminderOffset = reminder,
                    Priority = SelectedPriority.Value,
                    CourseId = courseId,
                    EstimatedMinutes = estimatedMinutes,
                    ActivityType = ActivityType,
                    PlannedStartDate = HasPlannedStart ? PlannedStartDate.Date : null,
                    FirstStep = Clean(FirstStep),
                    PlannedReviewDate = IsDeliverable && HasPlannedReview ? PlannedReviewDate.Date : null,
                    Subtasks = Subtasks.Where(x => !string.IsNullOrWhiteSpace(x.Title)).Select(x => new SubtaskDto
                    {
                        Id = x.Id,
                        Title = x.Title.Trim(),
                        IsCompleted = x.IsCompleted,
                        PlannedDate = x.PlannedDate
                    }).ToList()
                });
                if (!result.IsSuccess)
                {
                    ErrorMessage = result.Message ?? "No se pudo actualizar la actividad.";
                    return;
                }
            }
            else
            {
                var result = await _tasks.CreateTaskAsync(new CreateTaskCommand
                {
                    Title = Title.Trim(),
                    Description = Clean(Description),
                    DueDate = dueDate,
                    ReminderOffset = reminder,
                    Priority = SelectedPriority.Value,
                    CourseId = courseId,
                    EstimatedMinutes = estimatedMinutes,
                    ActivityType = ActivityType,
                    PlannedStartDate = HasPlannedStart ? PlannedStartDate.Date : null,
                    FirstStep = Clean(FirstStep),
                    PlannedReviewDate = IsDeliverable && HasPlannedReview ? PlannedReviewDate.Date : null,
                    Subtasks = IsExam ? subtaskTitles : null,
                    Milestones = IsExam ? null : subtaskTitles.Select(x => new PlannedStepDto { Title = x }).ToList()
                });
                if (!result.IsSuccess)
                {
                    ErrorMessage = result.Message ?? "No se pudo crear la actividad.";
                    return;
                }
            }

            await ShellNavigation.GoBackAsync();
        }
        catch (Exception exception)
        {
            ErrorMessage = "No se pudo guardar. Revisa los datos e inténtalo nuevamente.";
            System.Diagnostics.Trace.WriteLine($"[PS-TASK-EDITOR] save: {exception}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool Validate(out int? estimatedMinutes)
    {
        estimatedMinutes = null;
        TitleError = string.IsNullOrWhiteSpace(Title) ? "Escribe un título para continuar." : null;
        if (HasTitleError)
            return false;

        if (!string.IsNullOrWhiteSpace(EstimatedMinutesText))
        {
            if (!int.TryParse(EstimatedMinutesText, out var minutes) || minutes <= 0)
            {
                ErrorMessage = "El tiempo estimado debe ser un número mayor que cero.";
                return false;
            }
            estimatedMinutes = minutes;
        }

        if (IsExam && !HasDueDate)
        {
            ErrorMessage = "El examen necesita fecha y hora.";
            return false;
        }
        return true;
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed record TaskPriorityOption(TaskPriority Value, string Label);
public sealed record ReminderOption(ReminderOffset Value, string Label);
public sealed record SubtaskDraft(Guid Id, string Title, bool IsCompleted, DateTime? PlannedDate);
