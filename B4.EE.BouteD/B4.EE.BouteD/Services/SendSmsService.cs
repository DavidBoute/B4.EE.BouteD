using B4.EE.BouteD.Models;
using Plugin.Messaging;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using System;
using System.Threading.Tasks;

namespace B4.EE.BouteD.Services
{
    public class SendSmsService
    {
        private bool _canSend;

        public async Task<string> Send(SmsDTO sms)
        {
            var smsMessenger = CrossMessaging.Current.SmsMessenger;
            string newStatus = "";

            if (smsMessenger.CanSendSmsInBackground)
            {
                try
                {
                    var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Sms);
                    if (status != PermissionStatus.Granted)
                    {
                        var results = await CrossPermissions.Current.RequestPermissionsAsync(Permission.Sms);

                        //Best practice to always check that the key exists
                        if (results.ContainsKey(Permission.Sms))
                        {
                            status = results[Permission.Sms];
                        }
                    }

                    if (status == PermissionStatus.Granted)
                    {
                        if (_canSend)
                        {
                            smsMessenger.SendSmsInBackground(sms.ContactNumber, sms.Message);
                        }
                        else
                        {
                            newStatus = "Sent";
                        }
                    }

                }
                catch (Exception)
                {
                    newStatus = "Error";
                }
            }
            else
            {
                newStatus = "Error";
            }

            return newStatus;
        }


        public SendSmsService(bool CanSend)
        {
            _canSend = CanSend;
        }
    }
}
