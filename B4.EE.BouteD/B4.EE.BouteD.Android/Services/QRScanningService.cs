using B4.EE.BouteD.Services;
using System.Threading.Tasks;
using Xamarin.Forms;
using ZXing.Mobile;

[assembly: Dependency(typeof(B4.EE.BouteD.Droid.Services.QRScanningService))]

namespace B4.EE.BouteD.Droid.Services
{
    public class QRScanningService : IQRScanningService
    {
        public async Task<string> ScanAsync()
        {
            var optionsDefault = new MobileBarcodeScanningOptions();
            var optionsCustom = new MobileBarcodeScanningOptions();

            var scanner = new MobileBarcodeScanner()
            {
                TopText = "Scan the QR Code",
                BottomText = "Please Wait",
            };

            try
            {
                var scanResult = await scanner.Scan();
                return scanResult.Text;
            }
            catch (System.Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Fout", ex.Message, "OK");
                return string.Empty;
            } 
        }
    }
} 
