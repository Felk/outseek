using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace AvaloniaOutseekClient.Converters
{
    /// <summary>
    /// This converter interprets the linear values of a slider as non-linear zoom factors.
    /// The specific values were obtained by just tweaking them until the zoom slider's behaviour felt about right. 
    /// </summary>
    public class ZoomPowConverter : IValueConverter
    {
        public const double Exponent = 7d;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double devicePixelsPerSecond = (double) value;
            return Math.Round(Math.Pow(devicePixelsPerSecond, 1d / Exponent), 2);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double sliderValue = (double) value;
            return Math.Round(Math.Pow(sliderValue, Exponent), 2);
        }
    }
}
