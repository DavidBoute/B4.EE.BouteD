using System;
using System.Globalization;
using Xamarin.Forms;

namespace SmsSenderApp.Converters
{
    class SendStatusBoolToColorConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                bool IsActive = (bool)value;
                if (IsActive)
                {    
                    return Color.FromHex("#d9534f"); // red
                }
                else
                {
                    return Color.FromHex("#5cb85c"); // green
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
