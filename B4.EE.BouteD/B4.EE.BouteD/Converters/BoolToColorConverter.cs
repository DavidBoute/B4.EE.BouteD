using System;
using System.Globalization;
using Xamarin.Forms;

namespace B4.EE.BouteD.Converters
{
    class BoolToColorConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                bool IsActive = (bool)value;
                if (IsActive)
                {
                    return "#5cb85c"; // green
                }
                else
                {
                    return "#d9534f"; // red
                }
            }
            catch (Exception)
            {
                throw new ArgumentException();
            }
            
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
