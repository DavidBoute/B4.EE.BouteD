using System;
using System.Globalization;
using Xamarin.Forms;

namespace B4.EE.BouteD.Converters
{
    class StatusToColorConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value.ToString().ToLower())
            {
                case "created":
                    return "#FFFFFF"; // white
                case "queued":
                    return "#0275d8"; // blue
                case "pending":
                    return "#f0ad4e"; // orange
                case "sent":
                    return "#5cb85c"; // green
                case "error":
                    return "#d9534f"; // red
                default:
                    return "#FFFFFF"; //white
            }
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
