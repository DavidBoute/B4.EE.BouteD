using SmsSenderApp.Services.Abstract;
using System.Threading.Tasks;
using Xamarin.Forms;
using ZXing.Mobile;

[assembly: Dependency(typeof(SmsSenderApp.Droid.Services.QRScanningService))]

namespace SmsSenderApp.Droid.Services
{
    public class QRScanningService : IQRScanningService
    {
        public async Task<string> ScanAsync()
        {
            var scanner = new MobileBarcodeScanner()
            {
                CancelButtonText = "Cancel",
                FlashButtonText = "Flash"
            };

            var customOverlay = new ZxingOverlayView(Android.App.Application.Context, scanner);
            scanner.CustomOverlay = customOverlay;
            scanner.UseCustomOverlay = true;

            try
            {
                var scanResult = await scanner.Scan();
                return (scanResult == null) ? string.Empty : scanResult.Text;
            }
            catch (System.Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Fout", ex.Message, "OK");
                return string.Empty;
            } 
        }


    }
} 
