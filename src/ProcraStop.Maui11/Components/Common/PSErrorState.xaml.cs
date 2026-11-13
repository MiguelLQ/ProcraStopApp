using System.Windows.Input;

namespace ProcraStop.Maui11.Components.Common;

/// <summary>
/// Estado de error: mensaje claro + acción de reintento (obligatorio en
/// toda pantalla de datos; jamás un toast que desaparece).
/// Ejemplo:
/// <code>
/// &lt;common:PSErrorState Title="No se pudo cargar"
///                      Message="Revisa tu conexión e inténtalo de nuevo."
///                      RetryCommand="{Binding RetryCommand}" /&gt;
/// </code>
/// </summary>
public partial class PSErrorState : ContentView
{
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(PSErrorState), default(string));

    public static readonly BindableProperty MessageProperty =
        BindableProperty.Create(nameof(Message), typeof(string), typeof(PSErrorState), default(string));

    public static readonly BindableProperty RetryTextProperty =
        BindableProperty.Create(nameof(RetryText), typeof(string), typeof(PSErrorState), "Reintentar");

    public static readonly BindableProperty RetryCommandProperty =
        BindableProperty.Create(nameof(RetryCommand), typeof(ICommand), typeof(PSErrorState), default(ICommand));

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

    public string RetryText
    {
        get => (string)GetValue(RetryTextProperty);
        set => SetValue(RetryTextProperty, value);
    }

    public ICommand RetryCommand
    {
        get => (ICommand)GetValue(RetryCommandProperty);
        set => SetValue(RetryCommandProperty, value);
    }

    public PSErrorState() => InitializeComponent();
}
