using B4.EE.BouteD.Models;
using B4.EE.BouteD.Services;
using FreshMvvm;
using Newtonsoft.Json;
using System;
using System.Windows.Input;
using Xamarin.Forms;

namespace B4.EE.BouteD.ViewModels
{
    public class SettingsViewModel : FreshBasePageModel
    {
        private string _prefix;
        public string Prefix
        {
            get { return _prefix; }
            set { _prefix = value; RaisePropertyChanged(); }
        }

        private string _host;
        public string Host
        {
            get { return _host; }
            set { _host = value; RaisePropertyChanged(); }
        }
        private string _port;
        public string Port
        {
            get { return _port; }
            set { _port = value; RaisePropertyChanged(); }
        }
        private string _path;
        public string Path
        {
            get { return _path; }
            set { _path = value; RaisePropertyChanged(); }
        }

        public ICommand GetQRCommand => new Command(
            async () =>
            {
                try
                {
                    var scanner = DependencyService.Get<IQRScanningService>();
                    var result = await scanner.ScanAsync();

                    ConnectionSettingsDTO settings = JsonConvert.DeserializeObject<ConnectionSettingsDTO>(result);
                    Prefix = settings.Prefix;
                    Host = settings.Host;
                    Port = settings.Port;
                    Path = settings.Path;
                }
                catch (Exception ex)
                {
                    await CoreMethods.DisplayAlert("Fout", ex.Message, "Cancel");
                }
            });

        public ICommand SaveSettingsCommand => new Command(
            () =>
            {
                ConnectionSettings.Prefix = Prefix;
                ConnectionSettings.Host = Host;
                ConnectionSettings.Port = Port;
                ConnectionSettings.Path = Path;

                CoreMethods.PopPageModel();
            });

        // Constructor
        public SettingsViewModel()
        {
            try
            {
                Prefix = ConnectionSettings.Prefix;
                Host = ConnectionSettings.Host;
                Port = ConnectionSettings.Port;
                Path = ConnectionSettings.Path;
            }
            catch (Exception)
            {
                throw;
            }

        }
    }
}
