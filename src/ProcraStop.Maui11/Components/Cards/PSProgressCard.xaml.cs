namespace ProcraStop.Maui11.Components.Cards;

/// <summary>
/// Card de progreso (hábitos/tareas): título + barra de progreso + %.
/// Ejemplo:
/// <code>
/// &lt;cards:PSProgressCard Title="Hábitos de hoy"
///                       Value="0.75" PercentageText="75%" /&gt;
/// </code>
/// Sin lógica: Value (0..1) y PercentageText llegan calculados del VM.
/// El ancho del fill es presentación pura (respuesta a Value).
/// </summary>
public partial class PSProgressCard : Border
{
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(PSProgressCard), default(string));

    public static readonly BindableProperty PercentageTextProperty =
        BindableProperty.Create(nameof(PercentageText), typeof(string), typeof(PSProgressCard), default(string));

    /// <summary>Progreso normalizado 0..1 (estado ya calculado).</summary>
    public static readonly BindableProperty ValueProperty =
        BindableProperty.Create(nameof(Value), typeof(double), typeof(PSProgressCard), 0d,
            coerceValue: CoerceValue, propertyChanged: OnValueChanged);

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string PercentageText
    {
        get => (string)GetValue(PercentageTextProperty);
        set => SetValue(PercentageTextProperty, value);
    }

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    private static object CoerceValue(BindableObject b, object value) =>
        Math.Clamp((double)value, 0d, 1d);

    private static void OnValueChanged(BindableObject b, object oldV, object newV)
    {
        if (b is PSProgressCard card)
            card.UpdateFill();
    }

    /// <summary>Recalcula el fill al medir la card (primer layout).</summary>
    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        UpdateFill();
    }

    // Presentación pura: el fill sigue a Value (0..1) sobre el ancho REAL
    // de la pista (no del card completo, que incluye el padding).
    private void UpdateFill()
    {
        if (FillBar is null)
            return;
        // Respaldo antes del primer layout: ancho del card menos padding 16×2.
        var usable = TrackBar.Width > 0 ? TrackBar.Width : Math.Max(0, Width - 32);
        FillBar.WidthRequest = Math.Max(0, usable * Value);
    }

    public PSProgressCard()
    {
        InitializeComponent();
        TrackBar.SizeChanged += (_, _) => UpdateFill();
    }
}
