using System.Windows.Input;

namespace ProcraStop.Maui11.Components.Common;

/// <summary>
/// Estado vacío (obligatorio en toda lista de datos): icono + título +
/// texto empático + acción sugerida opcional.
/// Ejemplo:
/// <code>
/// &lt;common:PSEmptyState Glyph="{x:Static fonts:FluentUI.clipboard_task_24_regular}"
///                      Title="Aún no hay tareas"
///                      Message="Crea tu primera tarea para empezar."
///                      ActionText="Crear tarea"
///                      Command="{Binding CreateTaskCommand}" /&gt;
/// </code>
/// </summary>
public partial class PSEmptyState : ContentView
{
    public static readonly BindableProperty GlyphProperty =
        BindableProperty.Create(nameof(Glyph), typeof(string), typeof(PSEmptyState), default(string));

    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(PSEmptyState), default(string));

    public static readonly BindableProperty MessageProperty =
        BindableProperty.Create(nameof(Message), typeof(string), typeof(PSEmptyState), default(string));

    public static readonly BindableProperty ActionTextProperty =
        BindableProperty.Create(nameof(ActionText), typeof(string), typeof(PSEmptyState), default(string));

    public static readonly BindableProperty CommandProperty =
        BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(PSEmptyState), default(ICommand));

    public string Glyph
    {
        get => (string)GetValue(GlyphProperty);
        set => SetValue(GlyphProperty, value);
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public string ActionText
    {
        get => (string)GetValue(ActionTextProperty);
        set => SetValue(ActionTextProperty, value);
    }

    public ICommand Command
    {
        get => (ICommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    /// <summary>La acción sugerida solo se muestra si hay texto.</summary>
    public bool HasAction => !string.IsNullOrEmpty(ActionText);

    public PSEmptyState() => InitializeComponent();
}
