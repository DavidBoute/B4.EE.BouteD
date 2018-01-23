using B4.EE.BouteD.Services;
using B4.EE.BouteD.Services.Abstract;
using B4.EE.BouteD.ViewModels;
using FreshMvvm;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace B4.EE.BouteD
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            FreshIOC.Container.Register<ISmsDataService, SmsFromSignalRService>();
            FreshIOC.Container.Register(SignalRService.Instance());
            FreshIOC.Container.Register(new ConnectionSettings());
            FreshIOC.Container.Register(new SendSmsService());

            var page = FreshPageModelResolver.ResolvePageModel<SmsViewModel>();
            var basicNavContainer = new FreshNavigationContainer(page);
            MainPage = basicNavContainer;
        }
    }
}