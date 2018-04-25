using System;
using System.Globalization;
using Xamarin.Forms;

namespace B4.EE.BouteD.Converters
{
    class SendStatusBoolToStringConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                bool IsActive = (bool)value;
                if (IsActive)
                {
                    return "Stop sending";
                }
                else
                {
                    return "Start sending";
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
