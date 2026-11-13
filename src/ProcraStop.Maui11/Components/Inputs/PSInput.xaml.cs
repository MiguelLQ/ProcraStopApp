namespace ProcraStop.Maui11.Components.Inputs;

/// <summary>
/// Input Material 3 (campo "filled" estilo Grial): label fijo + Entry +
/// estado de error (borde Error + mensaje debajo).
/// Ejemplo:
/// <code>
/// &lt;inputs:PSInput Label="Nombre de la tarea"
///                 Placeholder="Estudiar cálculo…"
///                 Text="{Binding Name, Mode=TwoWay}"
///                 HasError="{Binding HasError}"
///                 ErrorText="{Binding NameError}" /&gt;
/// </code>
/// </summary>
public partial class PSInput : Border
{
    public static readonly BindableProperty LabelProperty =
        BindableProperty.Create(nameof(Label), typeof(string), typeof(PSInput), default(string));

    public static readonly BindableProperty PlaceholderProperty =
        BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(PSInput), default(string));

    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(PSInput),
            default(string), BindingMode.TwoWay);

    public static readonly BindableProperty IsPasswordProperty =
        BindableProperty.Create(nameof(IsPassword), typeof(bool), typeof(PSInput), false);

    public static readonly BindableProperty HasErrorProperty =
        BindableProperty.Create(nameof(HasError), typeof(bool), typeof(PSInput), false);

    public static readonly BindableProperty ErrorTextProperty =
        BindableProperty.Create(nameof(ErrorText), typeof(string), typeof(PSInput), default(string));

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public bool IsPassword
    {
        get => (bool)GetValue(IsPasswordProperty);
        set => SetValue(IsPasswordProperty, value);
    }

    public bool HasError
    {
        get => (bool)GetValue(HasErrorProperty);
        set => SetValue(HasErrorProperty, value);
    }

    public string ErrorText
    {
        get => (string)GetValue(ErrorTextProperty);
        set => SetValue(ErrorTextProperty, value);
    }

    /// <summary>El label solo se muestra si hay texto.</summary>
    public bool HasLabel => !string.IsNullOrEmpty(Label);

    /// <summary>Alias de HasError para el Trigger interno del XAML.</summary>
    public bool IsError => HasError;

    public PSInput() => InitializeComponent();
}
