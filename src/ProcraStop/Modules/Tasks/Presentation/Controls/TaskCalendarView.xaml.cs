using Microsoft.Maui.Controls.Shapes;
using ProcraStop.Modules.Tasks.Presentation.Models;
using System.Windows.Input;

namespace ProcraStop.Modules.Tasks.Presentation.Controls;

public partial class TaskCalendarView : ContentView
{
    private readonly List<CalendarCell> _weekCells = new(7);
    private readonly List<CalendarCell> _monthCells = new(42);

    public TaskCalendarView()
    {
        InitializeComponent();
        BuildStaticCalendarCells(); // editbycodex: se crean las vistas una sola vez para mejorar respuesta al tocar/cambiar mes.
    }

    public static readonly BindableProperty VisibleMonthTextProperty =
        BindableProperty.Create(
            nameof(VisibleMonthText),
            typeof(string),
            typeof(TaskCalendarView),
            string.Empty);

    public string VisibleMonthText
    {
        get => (string)GetValue(VisibleMonthTextProperty);
        set => SetValue(VisibleMonthTextProperty, value);
    }

    public static readonly BindableProperty WeekDaysProperty =
        BindableProperty.Create(
            nameof(WeekDays),
            typeof(IEnumerable<CalendarDayItem>),
            typeof(TaskCalendarView),
            Array.Empty<CalendarDayItem>(),
            propertyChanged: OnDaysChanged);

    public IEnumerable<CalendarDayItem> WeekDays
    {
        get => (IEnumerable<CalendarDayItem>)GetValue(WeekDaysProperty);
        set => SetValue(WeekDaysProperty, value);
    }

    public static readonly BindableProperty MonthDaysProperty =
        BindableProperty.Create(
            nameof(MonthDays),
            typeof(IEnumerable<CalendarDayItem>),
            typeof(TaskCalendarView),
            Array.Empty<CalendarDayItem>(),
            propertyChanged: OnDaysChanged);

    public IEnumerable<CalendarDayItem> MonthDays
    {
        get => (IEnumerable<CalendarDayItem>)GetValue(MonthDaysProperty);
        set => SetValue(MonthDaysProperty, value);
    }

    public static readonly BindableProperty IsExpandedProperty =
        BindableProperty.Create(
            nameof(IsExpanded),
            typeof(bool),
            typeof(TaskCalendarView),
            false,
            propertyChanged: OnStateChanged);

    public bool IsExpanded
    {
        get => (bool)GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    public static readonly BindableProperty IsCompactVisibleProperty =
        BindableProperty.Create(
            nameof(IsCompactVisible),
            typeof(bool),
            typeof(TaskCalendarView),
            true,
            propertyChanged: OnStateChanged);

    public bool IsCompactVisible
    {
        get => (bool)GetValue(IsCompactVisibleProperty);
        set => SetValue(IsCompactVisibleProperty, value);
    }

    public static readonly BindableProperty SelectDateCommandProperty =
        BindableProperty.Create(
            nameof(SelectDateCommand),
            typeof(ICommand),
            typeof(TaskCalendarView),
            default(ICommand),
            propertyChanged: OnCommandChanged);

    public ICommand? SelectDateCommand
    {
        get => (ICommand?)GetValue(SelectDateCommandProperty);
        set => SetValue(SelectDateCommandProperty, value);
    }

    public static readonly BindableProperty ToggleCalendarCommandProperty =
        BindableProperty.Create(
            nameof(ToggleCalendarCommand),
            typeof(ICommand),
            typeof(TaskCalendarView),
            default(ICommand));

    public ICommand? ToggleCalendarCommand
    {
        get => (ICommand?)GetValue(ToggleCalendarCommandProperty);
        set => SetValue(ToggleCalendarCommandProperty, value);
    }

    public static readonly BindableProperty PreviousMonthCommandProperty =
        BindableProperty.Create(
            nameof(PreviousMonthCommand),
            typeof(ICommand),
            typeof(TaskCalendarView),
            default(ICommand));

    public ICommand? PreviousMonthCommand
    {
        get => (ICommand?)GetValue(PreviousMonthCommandProperty);
        set => SetValue(PreviousMonthCommandProperty, value);
    }

    public static readonly BindableProperty NextMonthCommandProperty =
        BindableProperty.Create(
            nameof(NextMonthCommand),
            typeof(ICommand),
            typeof(TaskCalendarView),
            default(ICommand));

    public ICommand? NextMonthCommand
    {
        get => (ICommand?)GetValue(NextMonthCommandProperty);
        set => SetValue(NextMonthCommandProperty, value);
    }

    private static void OnDaysChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is TaskCalendarView calendar)
            calendar.RenderAllCells(); // editbycodex: actualiza compacto y expandido para que no dependan de abrir/cerrar.
    }

    private static void OnStateChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is TaskCalendarView calendar)
            calendar.RenderAllCells(); // editbycodex: al alternar modo, las celdas ya llegan con datos actuales.
    }

    private static void OnCommandChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is TaskCalendarView calendar)
            calendar.UpdateCellCommands();
    }

    private void BuildStaticCalendarCells()
    {
        BuildWeekCells();
        BuildMonthCells();
        RenderAllCells();
    }

    private void BuildWeekCells()
    {
        WeekHost.Children.Clear();
        _weekCells.Clear();

        for (var index = 0; index < 7; index++)
        {
            var cell = CreateWeekCell();
            _weekCells.Add(cell);
            WeekHost.Children.Add(cell.Root);
        }
    }

    private void BuildMonthCells()
    {
        MonthGrid.Children.Clear();
        MonthGrid.RowDefinitions.Clear();
        MonthGrid.ColumnDefinitions.Clear();
        _monthCells.Clear();

        for (var i = 0; i < 6; i++)
            MonthGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        for (var i = 0; i < 7; i++)
            MonthGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

        for (var index = 0; index < 42; index++)
        {
            var cell = CreateMonthCell();
            _monthCells.Add(cell);

            Grid.SetRow(cell.Root, index / 7);
            Grid.SetColumn(cell.Root, index % 7);
            MonthGrid.Children.Add(cell.Root);
        }
    }

    private void RenderVisibleCells()
    {
        // editbycodex: solo actualiza el modo visible; reduce trabajo al alternar compacto/expandido.
        if (IsCompactVisible)
            RenderCells(_weekCells, WeekDays);

        if (IsExpanded)
            RenderCells(_monthCells, MonthDays);
    }

    private void RenderAllCells()
    {
        // editbycodex: 49 celdas fijas son baratas; mantener ambas vistas sincronizadas evita clicks atrasados.
        RenderCells(_weekCells, WeekDays);
        RenderCells(_monthCells, MonthDays);
    }

    private void RenderCells(IReadOnlyList<CalendarCell> cells, IEnumerable<CalendarDayItem> source)
    {
        var days = source as IReadOnlyList<CalendarDayItem> ?? source.ToList();

        for (var i = 0; i < cells.Count; i++)
        {
            var day = i < days.Count ? days[i] : null;
            UpdateCell(cells[i], day);
        }
    }

    private void UpdateCell(CalendarCell cell, CalendarDayItem? day)
    {
        cell.Root.IsVisible = day is not null;

        if (day is null)
            return;

        cell.DayName.Text = day.DayName;
        cell.DayName.IsVisible = cell.ShowDayName;
        cell.DayName.SetAppThemeColor(Label.TextColorProperty, GetColor("Subtitlecl"), GetColor("Subtitle"));
        cell.DayNumber.Text = day.DayNumber;
        cell.TaskDot.IsVisible = day.HasTasks;
        cell.OverdueDot.IsVisible = day.HasOverdueTasks; // editbycodex: rojo independiente para dias con vencidas.
        cell.NumberBorder.Opacity = day.IsCurrentMonth ? 1 : 0.35;
        cell.Tap.Command = SelectDateCommand;
        cell.Tap.CommandParameter = day;
        cell.RootTap.Command = SelectDateCommand; // editbycodex: toda la celda responde al toque, no solo el circulo.
        cell.RootTap.CommandParameter = day;

        if (day.IsSelected)
        {
            cell.NumberBorder.BackgroundColor = GetColor("primarioS");
            cell.NumberBorder.Opacity = 1;
            cell.DayNumber.TextColor = Colors.White;
            if (cell.ShowDayName)
                cell.DayName.TextColor = Colors.White;
            return;
        }

        if (day.IsToday)
        {
            // editbycodex: hoy usa un circulo suave; la seleccion azul solida conserva prioridad.
            cell.NumberBorder.BackgroundColor = GetTodayBackgroundColor();
            cell.NumberBorder.Opacity = 1;
            cell.DayNumber.TextColor = GetColor("primarioS");
            if (cell.ShowDayName)
                cell.DayName.TextColor = GetColor("primarioS");
            return;
        }

        cell.NumberBorder.BackgroundColor = Colors.Transparent;
        cell.DayNumber.SetAppThemeColor(Label.TextColorProperty, GetColor("Titlecl"), GetColor("Title"));
    }

    private void UpdateCellCommands()
    {
        foreach (var cell in _weekCells.Concat(_monthCells))
        {
            cell.Tap.Command = SelectDateCommand;
            cell.RootTap.Command = SelectDateCommand; // editbycodex: mantiene sincronizado el area tactil ampliada.
        }
    }

    private CalendarCell CreateWeekCell()
    {
        var root = new VerticalStackLayout
        {
            WidthRequest = 43,
            Spacing = 3,
            HorizontalOptions = LayoutOptions.Center
        };

        var dayName = CreateDayNameLabel();
        dayName.FontSize = 10;
        var numberBorder = CreateNumberBorder(43, 16);
        numberBorder.HeightRequest = 58;
        var dayNumber = CreateNumberLabel(14);
        var tap = new TapGestureRecognizer();
        var rootTap = new TapGestureRecognizer();
        var indicators = CreateIndicators(out var taskDot, out var overdueDot);

        var dateContent = new VerticalStackLayout
        {
            Spacing = 0,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };
        dateContent.Children.Add(dayNumber);
        dateContent.Children.Add(dayName);

        numberBorder.Content = dateContent;
        numberBorder.GestureRecognizers.Add(tap);
        root.GestureRecognizers.Add(rootTap); // editbycodex: mejora respuesta al tocar cualquier parte del dia.
        root.Children.Add(numberBorder);
        root.Children.Add(indicators);

        return new CalendarCell(root, dayName, numberBorder, dayNumber, taskDot, overdueDot, tap, rootTap, true);
    }

    private CalendarCell CreateMonthCell()
    {
        var root = new Grid
        {
            HeightRequest = 42,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Start
        };

        var dayName = CreateDayNameLabel();
        var numberBorder = CreateNumberBorder(36, 18);
        var dayNumber = CreateNumberLabel(13);
        var tap = new TapGestureRecognizer();
        var rootTap = new TapGestureRecognizer();
        var indicators = CreateIndicators(out var taskDot, out var overdueDot);

        dayName.IsVisible = false;
        numberBorder.Content = dayNumber;
        numberBorder.GestureRecognizers.Add(tap);
        root.GestureRecognizers.Add(rootTap); // editbycodex: mejora respuesta al tocar cualquier parte del dia.
        root.Children.Add(numberBorder);
        root.Children.Add(indicators);

        return new CalendarCell(root, dayName, numberBorder, dayNumber, taskDot, overdueDot, tap, rootTap, false);
    }

    private Label CreateDayNameLabel()
    {
        var label = new Label
        {
            FontSize = 11,
            FontFamily = "ManropeRegular",
            HorizontalTextAlignment = TextAlignment.Center
        };
        label.SetAppThemeColor(Label.TextColorProperty, GetColor("Subtitlecl"), GetColor("Subtitle"));
        return label;
    }

    private Border CreateNumberBorder(double size, double radius)
    {
        return new Border
        {
            WidthRequest = size,
            HeightRequest = size,
            StrokeThickness = 0,
            Padding = 0,
            BackgroundColor = Colors.Transparent,
            InputTransparent = true, // editbycodex: el tap se maneja en la celda completa para evitar doble toque.
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(radius) },
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };
    }

    private Label CreateNumberLabel(double fontSize)
    {
        var label = new Label
        {
            FontSize = fontSize,
            FontFamily = "MontserratSemiBold",
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center
        };
        label.SetAppThemeColor(Label.TextColorProperty, GetColor("Titlecl"), GetColor("Title"));
        return label;
    }

    private Grid CreateIndicators(out Border taskDot, out Border overdueDot)
    {
        var host = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Auto }
            },
            ColumnSpacing = 3,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.End,
            HeightRequest = 6
        };

        taskDot = CreateIndicatorDot(Color.FromArgb("#16A34A"));
        overdueDot = CreateIndicatorDot(Color.FromArgb("#DC2626"));
        Grid.SetColumn(overdueDot, 1);
        host.Children.Add(taskDot);
        host.Children.Add(overdueDot);

        return host; // editbycodex: host estable evita que mostrar badges cambie el tamaño de la celda.
    }

    private static Border CreateIndicatorDot(Color color)
    {
        return new Border
        {
            WidthRequest = 6,
            HeightRequest = 6,
            StrokeThickness = 0,
            BackgroundColor = color,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(3) },
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            IsVisible = false
        };
    }

    private Color GetColor(string resourceKey)
    {
        if (Microsoft.Maui.Controls.Application.Current?.Resources.TryGetValue(resourceKey, out var value) == true)
        {
            if (value is Color color)
                return color;

            if (value is SolidColorBrush brush)
                return brush.Color;

            if (value is string hex)
                return Color.FromArgb(hex);
        }

        return resourceKey switch
        {
            "primarioS" => Color.FromArgb("#2563EB"),
            "Titlecl" => Color.FromArgb("#111827"),
            "Title" => Colors.White,
            "Subtitlecl" => Color.FromArgb("#6B7280"),
            "Subtitle" => Color.FromArgb("#D1D5DB"),
            _ => Color.FromArgb("#111827")
        };
    }

    private static Color GetTodayBackgroundColor()
    {
        // editbycodex: contraste discreto y legible en ambos temas sin modificar recursos globales.
        return Microsoft.Maui.Controls.Application.Current?.RequestedTheme == AppTheme.Dark
            ? Color.FromArgb("#243B5A")
            : Color.FromArgb("#E6F0FF");
    }

    private sealed record CalendarCell(
        View Root,
        Label DayName,
        Border NumberBorder,
        Label DayNumber,
        Border TaskDot,
        Border OverdueDot,
        TapGestureRecognizer Tap,
        TapGestureRecognizer RootTap,
        bool ShowDayName);
}
