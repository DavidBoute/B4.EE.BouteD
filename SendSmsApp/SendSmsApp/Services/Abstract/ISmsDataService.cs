using SmsSenderApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmsSenderApp.Services.Abstract
{
    public interface ISmsDataService
    {
        void GetSmsList();
        void UpdateSms(SmsDTO sms);
        void DeleteSms(SmsDTO sms);

        void GetStatusList();
    }
}
