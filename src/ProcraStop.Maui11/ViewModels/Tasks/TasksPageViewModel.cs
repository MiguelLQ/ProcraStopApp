using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using ProcraStop.Application.Tasks.DTOs;
using ProcraStop.Application.Tasks.Facade;
using ProcraStop.Domain.Tasks.Enums;
using ProcraStop.Maui11.ViewModels.Common;
using ProcraStop.Maui11.Navigation;

namespace ProcraStop.Maui11.ViewModels.Tasks;

/// <summary>
/// Etapa blank: aquí vivirán la colección de tareas, filtros (chips),
/// comandos de completar/detalle y navegación a create-task.
/// </summary>
public partial class TasksPageViewModel : ObservableObject
{
    private readonly ITasksFacade _tasks;

    /// <summary>Estado del Bottom Sheet "Agregar" (FAB +).</summary>
    public AddSheetViewModel AddSheet { get; }
    public ObservableCollection<TaskListCard> Items { get; } = [];

    [ObservableProperty] public partial bool IsLoading { get; set; }
    [ObservableProperty] public partial string? ErrorMessage { get; set; }

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);
    public bool IsEmpty => !IsLoading && !HasError && Items.Count == 0;
    public bool HasItems => !IsLoading && !HasError && Items.Count > 0;

    public TasksPageViewModel(AddSheetViewModel addSheet, ITasksFacade tasks)
    {
        AddSheet = addSheet;
        _tasks = tasks;
    }

    /// <summary>Abre el Bottom Sheet "Agregar" (FAB +).</summary>
    [RelayCommand]
    private void OpenAddSheet() => AddSheet.IsOpen = true;

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (IsLoading)
            return;

        IsLoading = true;
        ErrorMessage = null;
        NotifyListStateChanged();
        try
        {
            var tasks = await _tasks.GetTasksAsync();
            Items.Clear();
            foreach (var task in tasks)
                Items.Add(TaskListCard.From(task));
        }
        catch (Exception exception)
        {
            ErrorMessage = "No pudimos cargar tus actividades.";
            System.Diagnostics.Trace.WriteLine($"[PS-TASKS] load: {exception}");
        }
        finally
        {
            IsLoading = false;
            NotifyListStateChanged();
        }
    }

    [RelayCommand]
    private Task EditTaskAsync(TaskListCard? task)
        => task is null
            ? Task.CompletedTask
            : ShellNavigation.GoToAsync(Routes.EditTaskEditor(task.Id));

    partial void OnErrorMessageChanged(string? value) => NotifyListStateChanged();

    private void NotifyListStateChanged()
    {
        OnPropertyChanged(nameof(HasError));
        OnPropertyChanged(nameof(IsEmpty));
        OnPropertyChanged(nameof(HasItems));
    }
}

public sealed record TaskListCard(Guid Id, string Title, string TypeLabel, string Meta, string DueLabel)
{
    public static TaskListCard From(TaskListItemDto task)
    {
        var type = task.ActivityType switch
        {
            AcademicActivityType.Reading => "Lectura",
            AcademicActivityType.Exam => "Examen",
            AcademicActivityType.Deliverable => "Entregable",
            _ => "Tarea"
        };
        var meta = string.IsNullOrWhiteSpace(task.CourseName) ? type : $"{type} · {task.CourseName}";
        var due = task.DueDate.HasValue ? $"Vence {task.DueDate:dd/MM · HH:mm}" : "Sin fecha límite";
        return new(task.Id, task.Title, type, meta, due);
    }
}
