using Plugin.Settings;
using Plugin.Settings.Abstractions;
using Xamarin.Forms;

namespace SmsSenderApp.Services
{
    /// <summary>
    /// Leest en schrijft waarden in:
    /// Android: SharedPreferences
    /// Apple: NSUserDefaults
    /// UWP: ApplicationDataContainer
    /// .NET: UserStore -> IsolcatedStorageFile
    /// </summary>
    public class ConnectionSettings
    {
        public string Prefix
        {
            get => AppSettings?.GetValueOrDefault(nameof(Prefix), string.Empty);
            set => AppSettings?.AddOrUpdateValue(nameof(Prefix), value);
        }
        public string Host
        {
            get => AppSettings?.GetValueOrDefault(nameof(Host), string.Empty);
            set => AppSettings?.AddOrUpdateValue(nameof(Host), value);
        }
        public string Port
        {
            get => AppSettings?.GetValueOrDefault(nameof(Port), string.Empty);
            set => AppSettings?.AddOrUpdateValue(nameof(Port), value);
        }
        public string Path
        {
            get => AppSettings?.GetValueOrDefault(nameof(Path), string.Empty);
            set => AppSettings?.AddOrUpdateValue(nameof(Path), value);
        }

        private ISettings AppSettings
        {
            get
            {
                if (CrossSettings.IsSupported)
                    return CrossSettings.Current;

                return null;
            }
        }

        /// <summary>
        /// Verwittigt andere klassen van update.
        /// </summary>
        public void NotifyUpdate()
        {
            MessagingCenter.Send(this, "UpdateConnectionSettings");
        }

        /// <summary>
        /// Past, indien nodig, waarden aan en geeft dan notificatie
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="path"></param>
        public void Update(string prefix, string host, string port, string path)
        {
            bool needNotify = false;

            if (prefix != Prefix)
            {
                Prefix = prefix;
                needNotify = true;
            }

            if (host != Host)
            {
                Host = host;
                needNotify = true;
            }

            if (port != Port)
            {
                Port = port;
                needNotify = true;
            }

            if (path != Path)
            {
                Path = path;
                needNotify = true;
            }

            if (needNotify)
            {
                NotifyUpdate();
            }
        }

        public ConnectionSettings()
        {
            // Set initial settings
            if (!AppSettings.Contains(nameof(Prefix))) Prefix = "http://";
            if (!AppSettings.Contains(nameof(Host))) Host = "192.168.1.4";
            if (!AppSettings.Contains(nameof(Port))) Port = "45455";
            if (!AppSettings.Contains(nameof(Path))) Path = "api/";
        }
    }
}
