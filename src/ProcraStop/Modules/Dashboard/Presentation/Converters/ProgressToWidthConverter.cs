
using System.Globalization;

namespace ProcraStop.Modules.Dashboard.Presentation.Converters
{
    public class ProgressToWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double maxWidth = 300;

            if (parameter != null && double.TryParse(parameter.ToString(), out var parsed))
                maxWidth = parsed;

            if (value is double v)
                return v * maxWidth;

            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
