using B4.EE.BouteD.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Xamarin.Forms;
using ZXing.Mobile;

[assembly: Dependency(typeof(B4.EE.BouteD.UWP.Services.QRScanningService))]

namespace B4.EE.BouteD.UWP.Services
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
                var dialog = new MessageDialog(ex.Message, "Fout");
                await dialog.ShowAsync();
                return string.Empty;
            }
        }
    }
}
