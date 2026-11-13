namespace ProcraStop.Maui11.Components.Indicators;

/// <summary>
/// Estado de carga: skeleton discreto (obligatorio en toda pantalla de
/// datos; nunca pantalla en blanco).
/// Ejemplo:
/// <code>
/// &lt;indicators:PSLoadingState Message="Cargando tus hábitos…" /&gt;
/// </code>
/// </summary>
public partial class PSLoadingState : ContentView
{
    public static readonly BindableProperty MessageProperty =
        BindableProperty.Create(nameof(Message), typeof(string), typeof(PSLoadingState), default(string));

    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public bool HasMessage => !string.IsNullOrEmpty(Message);

    public PSLoadingState() => InitializeComponent();
}
