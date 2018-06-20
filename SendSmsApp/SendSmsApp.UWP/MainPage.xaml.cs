namespace SmsSenderApp.UWP
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            this.InitializeComponent();
            LoadApplication(new SmsSenderApp.App());
        }
    }
}
