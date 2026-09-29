using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProcraStop.Application.Tasks.Commands.CompleteTask;
using ProcraStop.Application.Tasks.DTOs; // editbycodex: contratos ligeros para lista y resumen diario.
using ProcraStop.Application.Tasks.Queries.GetTasks;
using ProcraStop.Application.Tasks.Queries.GetTaskDaySummaries;
using ProcraStop.Domain.Tasks.Enums;
using ProcraStop.Modules.Tasks.Presentation.Models;
using ProcraStop.Shared.Services;
using ProcraStop.Shared.UI.Components;
using System.Collections.ObjectModel;
using System.Globalization;

namespace ProcraStop.Modules.Tasks.Presentation.ViewModels
{
    public partial class TasksPageViewModel : ObservableObject
    {
        private readonly GetTasksQueryHandler _tasksQuery;
        private readonly GetTaskDaySummariesQueryHandler _taskDaySummariesQuery;
        private readonly CompleteTaskCommandHandler _completeTaskCommand;
        private readonly ISnackbarService _snackbarService;
        private CancellationTokenSource? _searchDebounceCts;
        private CancellationTokenSource? _calendarSummaryCts; // editbycodex: cancela rangos antiguos al navegar rapido.
        private readonly SemaphoreSlim _queryGate = new(1, 1); // Evita recargas solapadas dentro de esta vista.
        private readonly HashSet<Guid> _completingTaskIds = new(); // editbycodex: evita doble completado por taps repetidos.

        private List<TaskItemModel> _allTasks = new();
        private IReadOnlyDictionary<DateTime, TaskDaySummaryDto> _daySummaries =
            new Dictionary<DateTime, TaskDaySummaryDto>(); // editbycodex: acceso O(1) al estado de cada dia.

        [ObservableProperty]
        private ObservableCollection<TaskItemModel> tasks = new();

        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private string searchText = string.Empty; // editbycodex: texto del buscador superior de TasksPage.

        [ObservableProperty]
        private bool isSearchActive; // editbycodex: estado visual para resultados de busqueda.

        [ObservableProperty]
        private string searchSummaryText = string.Empty; // editbycodex: resumen profesional debajo del buscador.

        [ObservableProperty]
        private TaskFilterOption selectedFilter = TaskFilterOption.All;

        public bool IsAllSelected => SelectedFilter == TaskFilterOption.All;
        public bool IsPendingSelected => SelectedFilter == TaskFilterOption.Pending;
        public bool IsCompletedSelected => SelectedFilter == TaskFilterOption.Completed;
        public bool IsOverdueSelected => SelectedFilter == TaskFilterOption.Overdue;

        public TasksPageViewModel(
            GetTasksQueryHandler tasksQuery,
            GetTaskDaySummariesQueryHandler taskDaySummariesQuery,
            CompleteTaskCommandHandler completeTaskCommand,
            ISnackbarService snackbarService)
        {
            _tasksQuery = tasksQuery;
            _taskDaySummariesQuery = taskDaySummariesQuery;
            _completeTaskCommand = completeTaskCommand;
            _snackbarService = snackbarService;
            BuildCalendar();
        }

        partial void OnSelectedFilterChanged(TaskFilterOption value)
        {
            OnPropertyChanged(nameof(IsAllSelected));
            OnPropertyChanged(nameof(IsPendingSelected));
            OnPropertyChanged(nameof(IsCompletedSelected));
            OnPropertyChanged(nameof(IsOverdueSelected));
        }

        partial void OnSearchTextChanged(string value)
        {
            _ = SearchWithDebounceAsync(value); // editbycodex: busqueda reactiva con debounce para no saturar la UI.
        }

        [RelayCommand]
        private Task SearchAsync()
        {
            return SearchWithDebounceAsync(SearchText, immediate: true); // editbycodex: soporta tecla Buscar del teclado.
        }

        [RelayCommand]
        public async Task LoadAsync()
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;

                var result = await ExecuteReadAsync(
                    ct => _tasksQuery.HandleAsync(
                        new GetTasksQuery { IncludeArchived = false },
                        ct),
                    CancellationToken.None);

                _allTasks = result
                    .Select(TaskItemModel.FromDto)
                    .ToList();

                if (string.IsNullOrWhiteSpace(SearchText))
                    ApplyFilter();
                else
                    await SearchNowAsync(SearchText, CancellationToken.None);

                await RefreshCalendarSummariesAsync(); // editbycodex: refresca badges al entrar o volver de crear/editar.
            }
            finally
            {
                IsBusy = false;
            }
        }
        [RelayCommand]
        private async Task OpenDetail(Guid taskId)
        {
            Console.WriteLine($" CLICK TASK: {taskId}");

            await Shell.Current.GoToAsync($"task-detail?taskId={taskId}");
        }

        [RelayCommand]
        private async Task CompleteFromList(Guid taskId)
        {
            var item = _allTasks.FirstOrDefault(x => x.Id == taskId);
            if (item is null || !item.CanQuickComplete || !_completingTaskIds.Add(taskId))
                return;

            try
            {
                await ExecuteOperationAsync(
                    ct => _completeTaskCommand.HandleAsync(
                        new CompleteTaskCommand { TaskId = taskId },
                        ct),
                    CancellationToken.None);

                var wasOverdue = item.IsActuallyOverdue;
                item.Status = TaskItemStatus.Completed;
                ApplyCompletedTaskIncrementally(item, wasOverdue);
                await _snackbarService.ShowAsync(
                    "¡La Tarea fue completada! Sigue Así 😉",
                    SnackbarType.Success);
            }
            catch (Exception ex)
            {
                await _snackbarService.ShowErrorAsync(
                    ex is InvalidOperationException
                        ? ex.Message
                        : "No se pudo completar la tarea. Inténtalo nuevamente.",
                    SnackbarType.Error); // editbycodex: no deja un check visual falso si la persistencia falla.
            }
            finally
            {
                _completingTaskIds.Remove(taskId);
            }
        }

        private void ApplyCompletedTaskIncrementally(TaskItemModel item, bool wasOverdue)
        {
            if (SelectedFilter is TaskFilterOption.Pending or TaskFilterOption.Overdue)
                Tasks.Remove(item);

            if (item.DueDate.HasValue &&
                _daySummaries.TryGetValue(item.DueDate.Value.Date, out var summary))
            {
                var updated = new TaskDaySummaryDto
                {
                    Date = summary.Date,
                    TaskCount = summary.TaskCount,
                    OverdueCount = wasOverdue
                        ? Math.Max(0, summary.OverdueCount - 1)
                        : summary.OverdueCount
                };

                var summaries = _daySummaries.ToDictionary(x => x.Key, x => x.Value);
                summaries[item.DueDate.Value.Date] = updated;
                _daySummaries = summaries;
                BuildCalendar();
            }

            UpdateSearchSummary(Tasks.Count, SearchText);
        }

        private void ApplyFilter()
        {
            // editbycodex: Sin fecha usa su propia bandeja; los demas filtros parten del dia seleccionado.
            IEnumerable<TaskItemModel> filtered = SelectedFilter == TaskFilterOption.WithoutDate
                ? _allTasks.Where(x => !x.DueDate.HasValue)
                : _allTasks.Where(IsTaskOnSelectedDate);

            filtered = SelectedFilter switch
            {
                TaskFilterOption.Pending => filtered.Where(x => x.Status != TaskItemStatus.Completed),
                TaskFilterOption.Completed => filtered.Where(x => x.Status == TaskItemStatus.Completed),
                TaskFilterOption.Overdue => filtered.Where(x => x.IsActuallyOverdue),
                _ => filtered
            };

            Tasks = new ObservableCollection<TaskItemModel>(filtered);
            UpdateSearchSummary(Tasks.Count, SearchText);
        }
        public ObservableCollection<TaskFilterChip> Filters { get; } = new()
        {
            new() { Title = "Todas", Filter = TaskFilterOption.All, IsSelected = true },
            new() { Title = "Pendientes", Filter = TaskFilterOption.Pending },
            new() { Title = "Completadas", Filter = TaskFilterOption.Completed },
            new() { Title = "Vencidas", Filter = TaskFilterOption.Overdue },
            new() { Title = "Sin fecha", Filter = TaskFilterOption.WithoutDate } // editbycodex: acceso a tareas no programadas.
        };

        [RelayCommand]
        private void ChangeFilter(TaskFilterOption filter)
        {
            if (SelectedFilter == filter)
                return;

            SelectedFilter = filter;

            foreach (var item in Filters)
                item.IsSelected = item.Filter == filter;

            if (string.IsNullOrWhiteSpace(SearchText))
                ApplyFilter(); // tu lógica actual
            else
                _ = SearchWithDebounceAsync(SearchText, immediate: true); // editbycodex: chips y busqueda trabajan juntos.
        }

        private async Task SearchWithDebounceAsync(string value, bool immediate = false)
        {
            _searchDebounceCts?.Cancel();
            _searchDebounceCts?.Dispose();
            _searchDebounceCts = new CancellationTokenSource();
            var token = _searchDebounceCts.Token;

            try
            {
                if (!immediate)
                    await Task.Delay(120, token); // editbycodex: respuesta mas agil sin procesar cada pulsacion.

                await SearchNowAsync(value, token);
            }
            catch (OperationCanceledException)
            {
            }
        }

        private Task SearchNowAsync(string value, CancellationToken ct)
        {
            var text = value.Trim();
            IsSearchActive = !string.IsNullOrWhiteSpace(text);

            if (!IsSearchActive)
            {
                SearchSummaryText = string.Empty;
                ApplyFilter();
                return Task.CompletedTask;
            }

            ct.ThrowIfCancellationRequested();

            // editbycodex: filtra el cache ya cargado; elimina viajes SQLite mientras el usuario escribe.
            IEnumerable<TaskItemModel> mapped = _allTasks.Where(task =>
                task.Title.Contains(text, StringComparison.CurrentCultureIgnoreCase) ||
                (!string.IsNullOrWhiteSpace(task.Description) &&
                 task.Description.Contains(text, StringComparison.CurrentCultureIgnoreCase)) ||
                (!string.IsNullOrWhiteSpace(task.CourseName) &&
                 task.CourseName.Contains(text, StringComparison.CurrentCultureIgnoreCase)));

            mapped = SelectedFilter switch
            {
                TaskFilterOption.Pending => mapped.Where(x => x.Status != TaskItemStatus.Completed),
                TaskFilterOption.Completed => mapped.Where(x => x.Status == TaskItemStatus.Completed),
                TaskFilterOption.Overdue => mapped.Where(x => x.IsActuallyOverdue),
                _ => mapped // editbycodex: Todas y Sin fecha no limitan una busqueda global.
            };

            Tasks = new ObservableCollection<TaskItemModel>(mapped.Take(40));
            UpdateSearchSummary(Tasks.Count, text); // editbycodex: feedback inmediato de resultados.
            return Task.CompletedTask;
        }

        private void UpdateSearchSummary(int count, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                SearchSummaryText = string.Empty;
                IsSearchActive = false;
                return;
            }

            IsSearchActive = true;
            SearchSummaryText = count == 1
                ? "1 resultado global"
                : $"{count} resultados globales"; // editbycodex: comunica que el calendario esta suspendido durante la busqueda.
        }

        private bool IsTaskOnSelectedDate(TaskItemModel task)
        {
            return task.DueDate.HasValue &&
                   task.DueDate.Value.Date == SelectedDate.Date; // editbycodex: compara el dia sin perder la hora.
        }
        [ObservableProperty]
        private IReadOnlyList<CalendarDayItem> weekDays = Array.Empty<CalendarDayItem>(); // editbycodex: lista nueva por render para disparar binding inmediato.

        [ObservableProperty]
        private IReadOnlyList<CalendarDayItem> monthDays = Array.Empty<CalendarDayItem>(); // editbycodex: evita Clear/Add sin notificar al control.

        private DateTime _selectedDate = DateTime.Today;
        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                if (SetProperty(ref _selectedDate, value))
                {
                    BuildCalendar();
                }
            }
        }

        private DateTime _visibleMonth = new(DateTime.Today.Year, DateTime.Today.Month, 1);
        public DateTime VisibleMonth
        {
            get => _visibleMonth;
            set
            {
                if (SetProperty(ref _visibleMonth, value))
                {
                    BuildCalendar();
                }
            }
        }

        private bool _isCalendarExpanded;
        public bool IsCalendarExpanded
        {
            get => _isCalendarExpanded;
            set
            {
                if (SetProperty(ref _isCalendarExpanded, value))
                {
                    OnPropertyChanged(nameof(IsCompactCalendarVisible));
                }
            }
        }

        public bool IsCompactCalendarVisible => !IsCalendarExpanded;

        public string VisibleMonthText =>
            VisibleMonth.ToString("MMMM yyyy", new CultureInfo("es-ES"));

        [RelayCommand]
        private void ToggleCalendar()
        {
            IsCalendarExpanded = !IsCalendarExpanded;

            if (IsCalendarExpanded)
            {
                var selectedMonth = new DateTime(
                    SelectedDate.Year,
                    SelectedDate.Month,
                    1);

                if (VisibleMonth.Year != selectedMonth.Year ||
                    VisibleMonth.Month != selectedMonth.Month)
                {
                    _visibleMonth = selectedMonth;
                    OnPropertyChanged(nameof(VisibleMonth));
                }
            }

            // editbycodex: al desplegar/compactar se sincronizan ambas vistas sin depender de CollectionChanged.
            BuildCalendar();
        }

        [RelayCommand]
        private void SelectDate(CalendarDayItem? day)
        {
            if (day is null)
                return;

            ExitWithoutDateMode(); // editbycodex: tocar un dia restaura el alcance diario de forma predecible.
            var selectedMonth = new DateTime(day.Date.Year, day.Date.Month, 1);

            _selectedDate = day.Date;
            OnPropertyChanged(nameof(SelectedDate));

            var monthChanged = false;
            if (VisibleMonth.Year != selectedMonth.Year || VisibleMonth.Month != selectedMonth.Month)
            {
                _visibleMonth = selectedMonth;
                monthChanged = true;
                OnPropertyChanged(nameof(VisibleMonth));
                OnPropertyChanged(nameof(VisibleMonthText));
            }

            // editbycodex: selecciona visualmente al instante en compacto y expandido.
            BuildCalendar(rebuildMonth: monthChanged || IsCalendarExpanded);

            RefreshTaskListForSelectedDate(); // editbycodex: el toque filtra la lista en el mismo frame.

            if (monthChanged)
                _ = RefreshCalendarSummariesAsync();
        }

        private void ExitWithoutDateMode()
        {
            if (SelectedFilter != TaskFilterOption.WithoutDate)
                return;

            SelectedFilter = TaskFilterOption.All;
            foreach (var item in Filters)
                item.IsSelected = item.Filter == TaskFilterOption.All;
        }

        [RelayCommand]
        private void PreviousMonth()
        {
            if (IsCalendarExpanded)
            {
                VisibleMonth = VisibleMonth.AddMonths(-1);
                _ = RefreshCalendarSummariesAsync(); // editbycodex: carga indicadores del nuevo rango visible.
                return;
            }

            SetSelectedDateFromCalendarNavigation(SelectedDate.AddDays(-7));
        }
        [RelayCommand]
        private void NextMonth()
        {
            if (IsCalendarExpanded)
            {
                VisibleMonth = VisibleMonth.AddMonths(1);
                _ = RefreshCalendarSummariesAsync(); // editbycodex: carga indicadores del nuevo rango visible.
                return;
            }

            SetSelectedDateFromCalendarNavigation(SelectedDate.AddDays(7));
        }
        private void SetSelectedDateFromCalendarNavigation(DateTime date)
        {
            _selectedDate = date.Date;
            OnPropertyChanged(nameof(SelectedDate));

            var selectedMonth = new DateTime(
                _selectedDate.Year,
                _selectedDate.Month,
                1);

            if (VisibleMonth.Year != selectedMonth.Year ||
                VisibleMonth.Month != selectedMonth.Month)
            {
                _visibleMonth = selectedMonth;
                OnPropertyChanged(nameof(VisibleMonth));
            }

            BuildCalendar();
            RefreshTaskListForSelectedDate(); // editbycodex: navegar semanas tambien cambia el filtro diario.
            _ = RefreshCalendarSummariesAsync();
        }

        private void RefreshTaskListForSelectedDate()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
                ApplyFilter();
            else
                _ = SearchWithDebounceAsync(SearchText, immediate: true);
        }

        private async Task RefreshCalendarSummariesAsync()
        {
            _calendarSummaryCts?.Cancel();
            _calendarSummaryCts?.Dispose();
            _calendarSummaryCts = new CancellationTokenSource();
            var token = _calendarSummaryCts.Token;

            var firstMonthDay = new DateTime(VisibleMonth.Year, VisibleMonth.Month, 1);
            var leadingDays = ((int)firstMonthDay.DayOfWeek + 6) % 7;
            var rangeStart = firstMonthDay.AddDays(-leadingDays);
            var rangeEnd = rangeStart.AddDays(41);

            try
            {
                var result = await ExecuteReadAsync(
                    ct => _taskDaySummariesQuery.HandleAsync(
                        new GetTaskDaySummariesQuery
                        {
                            StartDate = rangeStart,
                            EndDate = rangeEnd
                        },
                        ct),
                    token);

                _daySummaries = result.ToDictionary(x => x.Date.Date);
                BuildCalendar(); // editbycodex: repinta ambas vistas cuando llegan los conteos.
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async Task<T> ExecuteReadAsync<T>(
            Func<CancellationToken, Task<T>> operation,
            CancellationToken ct)
        {
            await _queryGate.WaitAsync(ct);
            try
            {
                return await operation(ct);
            }
            finally
            {
                _queryGate.Release(); // editbycodex: libera siempre la cola de lecturas, incluso al cancelar.
            }
        }

        private async Task ExecuteOperationAsync(
            Func<CancellationToken, Task> operation,
            CancellationToken ct)
        {
            await _queryGate.WaitAsync(ct);
            try
            {
                await operation(ct);
            }
            finally
            {
                _queryGate.Release(); // editbycodex: escritura y lecturas comparten la misma cola segura.
            }
        }
        private void BuildCalendar(bool rebuildMonth = true)
        {
            BuildWeekDays();

            if (rebuildMonth)
                BuildMonthDays();

            OnPropertyChanged(nameof(VisibleMonthText));
            OnPropertyChanged(nameof(IsCompactCalendarVisible));
        }

        private void BuildWeekDays()
        {
            var startOfWeek = SelectedDate.Date;
            var diff = ((int)startOfWeek.DayOfWeek + 6) % 7;
            startOfWeek = startOfWeek.AddDays(-diff);
            var days = new List<CalendarDayItem>(7); // editbycodex: reemplaza la lista completa para que MAUI actualice al primer toque.

            for (var i = 0; i < 7; i++)
            {
                var date = startOfWeek.AddDays(i);
                _daySummaries.TryGetValue(date.Date, out var summary);

                days.Add(new CalendarDayItem
                {
                    Date = date,
                    DayName = GetSpanishShortDayName(date),
                    DayNumber = date.Day.ToString(),
                    IsSelected = date.Date == SelectedDate.Date,
                    IsToday = date.Date == DateTime.Today, // editbycodex: referencia visual independiente de la seleccion.
                    IsCurrentMonth = date.Month == VisibleMonth.Month,
                    TaskCount = summary?.TaskCount ?? 0,
                    HasTasks = summary is not null && summary.TaskCount > summary.OverdueCount,
                    HasOverdueTasks = summary?.OverdueCount > 0
                });
            }

            WeekDays = days;
        }

        private void BuildMonthDays()
        {
            var firstDay = new DateTime(VisibleMonth.Year, VisibleMonth.Month, 1);
            var diff = ((int)firstDay.DayOfWeek + 6) % 7;
            var calendarStart = firstDay.AddDays(-diff);
            var days = new List<CalendarDayItem>(42); // editbycodex: nueva instancia para actualizar contenido al cambiar mes.

            for (var i = 0; i < 42; i++)
            {
                var date = calendarStart.AddDays(i);
                _daySummaries.TryGetValue(date.Date, out var summary);

                days.Add(new CalendarDayItem
                {
                    Date = date,
                    DayName = GetSpanishShortDayName(date),
                    DayNumber = date.Day.ToString(),
                    IsSelected = date.Date == SelectedDate.Date,
                    IsToday = date.Date == DateTime.Today, // editbycodex: visible tambien en calendario expandido.
                    IsCurrentMonth = date.Month == VisibleMonth.Month,
                    TaskCount = summary?.TaskCount ?? 0,
                    HasTasks = summary is not null && summary.TaskCount > summary.OverdueCount,
                    HasOverdueTasks = summary?.OverdueCount > 0
                });
            }

            MonthDays = days;
        }

        private static string GetSpanishShortDayName(DateTime date)
        {
            return date.DayOfWeek switch
            {
                DayOfWeek.Monday => "lun",
                DayOfWeek.Tuesday => "mar",
                DayOfWeek.Wednesday => "mié",
                DayOfWeek.Thursday => "jue",
                DayOfWeek.Friday => "vie",
                DayOfWeek.Saturday => "sáb",
                DayOfWeek.Sunday => "dom",
                _ => string.Empty
            };
        }

    }
}
