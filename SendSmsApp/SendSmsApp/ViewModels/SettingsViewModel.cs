using SmsSenderApp.Services;
using SmsSenderApp.Services.Abstract;
using FreshMvvm;
using Newtonsoft.Json;
using System;
using System.Windows.Input;
using Xamarin.Forms;

namespace SmsSenderApp.ViewModels
{
    public class SettingsViewModel : FreshBasePageModel
    {
        private ConnectionSettings _connectionSettings;

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

                    if (result != String.Empty)
                    {
                        Models.ConnectionSettingsDTO settings = JsonConvert.DeserializeObject<Models.ConnectionSettingsDTO>(result);
                        Prefix = settings.Prefix;
                        Host = settings.Host;
                        Port = settings.Port;
                        Path = settings.Path;
                    }
                }
                catch (Exception ex)
                {
                    await CoreMethods.DisplayAlert("Fout", ex.Message, "Cancel");
                }
            });

        public ICommand SaveSettingsCommand => new Command(
            () =>
            {
                _connectionSettings.Update(Prefix, Host, Port, Path);

                CoreMethods.PopPageModel();
            });

        // Constructor
        public SettingsViewModel(ConnectionSettings connectionSettings)
        {
            _connectionSettings = connectionSettings;

            Prefix = connectionSettings.Prefix;
            Host = connectionSettings.Host;
            Port = connectionSettings.Port;
            Path = connectionSettings.Path;
        }
    }
}
