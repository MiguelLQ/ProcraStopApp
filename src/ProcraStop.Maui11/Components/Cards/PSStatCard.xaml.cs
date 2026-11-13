namespace ProcraStop.Maui11.Components.Cards;

/// <summary>
/// Card de estadística: valor destacado + label + delta opcional.
/// Ejemplo:
/// <code>
/// &lt;cards:PSStatCard Value="1240" Label="Minutos de foco"
///                   DeltaText="+12%" IsPositive="True" /&gt;
/// </code>
/// Sin lógica de negocio: DeltaText/IsPositive llegan calculados; el color
/// del delta (Success/Error) se resuelve en XAML vía DataTrigger. El delta
/// se oculta cuando DeltaText es nulo o vacío.
/// </summary>
public partial class PSStatCard : Border
{
    public static readonly BindableProperty ValueProperty =
        BindableProperty.Create(nameof(Value), typeof(string), typeof(PSStatCard), default(string));

    public static readonly BindableProperty LabelProperty =
        BindableProperty.Create(nameof(Label), typeof(string), typeof(PSStatCard), default(string));

    public static readonly BindableProperty DeltaTextProperty =
        BindableProperty.Create(nameof(DeltaText), typeof(string), typeof(PSStatCard), default(string));

    public static readonly BindableProperty IsPositiveProperty =
        BindableProperty.Create(nameof(IsPositive), typeof(bool), typeof(PSStatCard), true);

    public string Value
    {
        get => (string)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public string DeltaText
    {
        get => (string)GetValue(DeltaTextProperty);
        set => SetValue(DeltaTextProperty, value);
    }

    public bool IsPositive
    {
        get => (bool)GetValue(IsPositiveProperty);
        set => SetValue(IsPositiveProperty, value);
    }

    /// <summary>Muestra el delta solo si hay texto (estado precalculado).</summary>
    public bool ShowDelta => !string.IsNullOrEmpty(DeltaText);

    public PSStatCard() => InitializeComponent();
}
