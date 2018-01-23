using B4.EE.BouteD.Models;
using B4.EE.BouteD.Services.Abstract;

namespace B4.EE.BouteD.Services
{
    // Return data wordt via MessagingCenter doorgegeven aan Viewmodels
    // Dit is omdat wijzigingen vanuit andere applicaties ook via die weg worden verwerkt
    public class SmsFromSignalRService : ISmsDataService
    {
        private SignalRService _signalRService;

        public void GetSmsList()
        {
            _signalRService.RequestSmsList();
        }

        public void GetStatusList()
        {
            _signalRService.RequestStatusList();
        }

        public void UpdateSms(SmsDTO sms)
        {
            _signalRService.RequestUpdateSms(sms);
        }

        public void DeleteSms(SmsDTO sms)
        {
            _signalRService.RequestDeleteSms(sms);
        }

        public SmsFromSignalRService()
        {
            _signalRService = SignalRService.Instance();
        }

    }
}
