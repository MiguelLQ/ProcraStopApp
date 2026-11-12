using System.Globalization;

namespace ProcraStop.Shared.UI.Convertes
{
    public class InverseBoolConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // Un binding puede entregar null mientras el BindingContext aún no
            // está resuelto. Convertir null a bool con un cast lanzaba
            // NullReferenceException y tumbaba la página.
            return value is bool flag ? !flag : true;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value is bool flag ? !flag : true;
        }
    }
}
