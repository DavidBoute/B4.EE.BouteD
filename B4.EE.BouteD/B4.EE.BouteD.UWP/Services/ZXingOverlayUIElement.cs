using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using ZXing.Mobile;

namespace B4.EE.BouteD.UWP.Services
{
    class ZXingOverlayUIElement : Grid
    {
        public ZXingOverlayUIElement(MobileBarcodeScanner scanner)
        {
            Background = new SolidColorBrush(Colors.Transparent);

            this.RowDefinitions.Add(new RowDefinition());
            this.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });
            this.ColumnDefinitions.Add(new ColumnDefinition());
            this.ColumnDefinitions.Add(new ColumnDefinition());

            // Cancel knop
            var btnCancel = new Button()
            {
                Background = new SolidColorBrush(Colors.Black),
                Content = scanner.CancelButtonText,
                Foreground = new SolidColorBrush(Colors.White),
                BorderBrush = new SolidColorBrush(Colors.White),
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            btnCancel.Click += (s, e2) =>
            {
                scanner.Cancel();
            };
            this.Children.Add(btnCancel);
            Grid.SetRow(btnCancel, 1);
            Grid.SetColumn(btnCancel, 0);

            // Knop voor flash
            var btnFlash = new Button()
            {
                Background = new SolidColorBrush(Colors.Black),
                Content = scanner.FlashButtonText,
                Foreground = new SolidColorBrush(Colors.White),
                BorderBrush = new SolidColorBrush(Colors.White),
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            btnFlash.Click += (s, e2) =>
            {
                scanner.ToggleTorch();
            };
            this.Children.Add(btnFlash);
            Grid.SetRow(btnFlash, 1);
            Grid.SetColumn(btnFlash, 1);
        }
    }
}
