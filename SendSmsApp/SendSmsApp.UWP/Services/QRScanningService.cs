using SmsSenderApp.Services.Abstract;
using System;
using System.Threading.Tasks;
using Windows.UI.Popups;
using ZXing.Mobile;

[assembly: Xamarin.Forms.Dependency(typeof(SmsSenderApp.UWP.Services.QRScanningService))]

namespace SmsSenderApp.UWP.Services
{
    public class QRScanningService : IQRScanningService
    {
        public async Task<string> ScanAsync()
        {
            var optionsDefault = new MobileBarcodeScanningOptions();

            var scanner = new MobileBarcodeScanner()
            {
                CancelButtonText = "Cancel",
                FlashButtonText = "Flash"
            };

            scanner.CustomOverlay = new ZXingOverlayUIElement(scanner);
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

    }
}
