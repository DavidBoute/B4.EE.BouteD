using Plugin.Settings;
using Plugin.Settings.Abstractions;

namespace B4.EE.BouteD.Services
{
    public static class ConnectionSettings
    {
        public static string Prefix
        {
            get => AppSettings?.GetValueOrDefault(nameof(Prefix), string.Empty);
            set => AppSettings?.AddOrUpdateValue(nameof(Prefix), value);
        }
        public static string Host
        {
            get => AppSettings?.GetValueOrDefault(nameof(Host), string.Empty);
            set => AppSettings?.AddOrUpdateValue(nameof(Host), value);
        }
        public static string Port
        {
            get => AppSettings?.GetValueOrDefault(nameof(Port), string.Empty);
            set => AppSettings?.AddOrUpdateValue(nameof(Port), value);
        }
        public static string Path
        {
            get => AppSettings?.GetValueOrDefault(nameof(Path), string.Empty);
            set => AppSettings?.AddOrUpdateValue(nameof(Path), value);
        }

        private static ISettings AppSettings
        {
            get
            {
                if (CrossSettings.IsSupported)
                    return CrossSettings.Current;

                return null; // or your custom implementation 
            }
        }

        static ConnectionSettings()
        {
            // Set initial settings
            if (!AppSettings.Contains(nameof(Prefix))) Prefix = "http://";
            if (!AppSettings.Contains(nameof(Host))) Host = "192.168.1.4";
            if (!AppSettings.Contains(nameof(Port))) Port = "45456";
            if (!AppSettings.Contains(nameof(Path))) Path = "api/";
        }
    }
}
