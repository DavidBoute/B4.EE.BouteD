using SmsSenderApp.Constants;
using System;
using System.Globalization;
using Xamarin.Forms;

namespace SmsSenderApp.Converters
{
    class ConnectionStateToImageSourceConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value.ToString())
            {
                case SignalRConnectionState.Open:
                    return OnDevice("wifi_button_green.png");
                case SignalRConnectionState.Slow:
                    return OnDevice("wifi_button_orange.png");
                case SignalRConnectionState.Reconnecting:
                    return OnDevice("wifi_button_orange.png");
                case SignalRConnectionState.Closed:
                    return OnDevice("wifi_button_red.png");
                case SignalRConnectionState.Error:
                    return OnDevice("wifi_button_red.png");
                default:
                    return OnDevice("wifi_button_red.png");
            }
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        string OnDevice(string source)
        {
            if (Device.RuntimePlatform == Device.UWP
                || Device.RuntimePlatform == Device.WinRT)
            {
                source = "Images/" + source;
            }
            return source;
        }
    }
}
