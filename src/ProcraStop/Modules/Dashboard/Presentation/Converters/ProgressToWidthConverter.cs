using System.Globalization;

namespace ProcraStop.Modules.Dashboard.Presentation.Converters
{
    /// <summary>
    /// Convierte un progreso normalizado (0–1) en un ancho en píxeles.
    /// </summary>
    public class ProgressToWidthConverter : IValueConverter
    {
        private const double DefaultMaxWidth = 300;

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var maxWidth = DefaultMaxWidth;

            if (parameter is not null &&
                double.TryParse(parameter.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
            {
                maxWidth = parsed;
            }

            if (value is double progress)
                return Math.Clamp(progress, 0, 1) * maxWidth;

            return 0d;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // La conversión inversa no tiene un significado útil: el ancho se
            // deriva del progreso, no al contrario. Devolver DoNothing evita el
            // NotImplementedException que rompía cualquier binding TwoWay.
            return Binding.DoNothing;
        }
    }
}
