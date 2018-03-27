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
                    return Color.FromHex("#FFFFFF"); // white
                case "queued":
                    return Color.FromHex("#0275d8"); // blue
                case "pending":
                    return Color.FromHex("#f0ad4e"); // orange
                case "sent":
                    return Color.FromHex("#5cb85c"); // green
                case "error":
                    return Color.FromHex("#d9534f"); // red
                default:
                    return Color.FromHex("#FFFFFF"); //white
            }
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
