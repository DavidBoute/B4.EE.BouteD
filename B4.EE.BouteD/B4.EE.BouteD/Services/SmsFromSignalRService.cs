using B4.EE.BouteD.Services.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using B4.EE.BouteD.Models;
using System.Collections.ObjectModel;

namespace B4.EE.BouteD.Services
{
    public class SmsFromSignalRService : ISmsService
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

        // Singleton implementation
        #region Singleton implementation
        private static SmsFromSignalRService _instance
            = new SmsFromSignalRService();

        public static SmsFromSignalRService Instance()
        {
            if (_instance == null)
            {
                _instance = new SmsFromSignalRService();
            }

            return _instance;
        }

        // private constructor
        private SmsFromSignalRService()
        {
            _signalRService = SignalRService.Instance();
        }
        #endregion

    }
}
