using B4.EE.BouteD.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using ZXing.Mobile;

[assembly: Xamarin.Forms.Dependency(typeof(B4.EE.BouteD.UWP.Services.QRScanningService))]

namespace B4.EE.BouteD.UWP.Services
{
    public class QRScanningService : IQRScanningService
    {
        public async Task<string> ScanAsync()
        {
            var optionsDefault = new MobileBarcodeScanningOptions();

            var scanner = new MobileBarcodeScanner()
            {
                TopText = "Scan the QR Code",
                BottomText = "Please Wait",
            };

            scanner.CustomOverlay = CreateCustomOverlay(scanner);
            scanner.UseCustomOverlay = true;

            try
            {
                var scanResult = await scanner.Scan();
                return (scanResult == null) ? string.Empty : scanResult.Text;
            }
            catch (System.Exception ex)
            {
                var dialog = new MessageDialog(ex.Message, "Fout");
                await dialog.ShowAsync();
                return string.Empty;
            }
        }

        private UIElement CreateCustomOverlay(MobileBarcodeScanner scanner)
        {
            var customOverlay = new Grid()
            {
                Background = new SolidColorBrush(Colors.Transparent),
            };
            customOverlay.RowDefinitions.Add(new RowDefinition());
            customOverlay.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });
            customOverlay.ColumnDefinitions.Add(new ColumnDefinition());
            customOverlay.ColumnDefinitions.Add(new ColumnDefinition());

            // Cancel knop
            var btnCancel = new Button()
            {
                Background = new SolidColorBrush(Colors.Black),
                Content = "Cancel",
                Foreground = new SolidColorBrush(Colors.White),
                BorderBrush = new SolidColorBrush(Colors.White),
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            btnCancel.Click += (s, e2) =>
            {
                scanner.Cancel();
            };
            customOverlay.Children.Add(btnCancel);
            Grid.SetRow(btnCancel, 1);
            Grid.SetColumn(btnCancel, 1);

            // Knop voor flash
            var btnFlash = new Button()
            {
                Background = new SolidColorBrush(Colors.Black),
                Content = "Flash",
                Foreground = new SolidColorBrush(Colors.White),
                BorderBrush = new SolidColorBrush(Colors.White),
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            btnFlash.Click += (s, e2) =>
            {
                scanner.ToggleTorch();
            };
            customOverlay.Children.Add(btnFlash);
            Grid.SetRow(btnFlash, 1);
            Grid.SetColumn(btnFlash, 0);

            var customOverlayPage = new Page();
            customOverlayPage.Content = customOverlay;
            customOverlayPage.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;

            return customOverlayPage;
        }
    }
}
