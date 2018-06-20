using Xamarin.Forms;

namespace SmsSenderApp.Models
{
    public class ConnectionSettingsDTO
    {
        public string Prefix { get; set; }
        public string Host { get; set; }
        public string Port { get; set; }
        public string Path { get; set; }
    }
}
