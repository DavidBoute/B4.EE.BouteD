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
        public string ErrorMessage { get; set; } = "";

        private bool _isSendEnabled;
        public bool IsSendEnabled
        {
            get { return _isSendEnabled; }
            set { _isSendEnabled = value; }
        }

        public SendSmsService CanSend(bool canSend)
        {
            _isSendEnabled = canSend;
            return this;
        }

        public async Task<bool> CheckPermissionAsync()
        {
            bool permissionGranted = false;

            var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Sms);
            if (status != PermissionStatus.Granted)
            {
                try
                {
                    var results = await CrossPermissions.Current.RequestPermissionsAsync(Permission.Sms);

                    //Best practice to always check that the key exists
                    if (results.ContainsKey(Permission.Sms))
                    {
                        status = results[Permission.Sms];
                    }

                    if (status == PermissionStatus.Granted)
                    {
                        permissionGranted = true;
                    }
                }
                catch (TaskCanceledException) 
                {
                    // Nog een fout in instellingen Plugin.Permissions
                    // Bij vragen toestemming in Android altijd fout "A Task was canceled"
                    // TODO: verder zoeken naar oplossing
                    // waarschijnlijk iets te maken met referentie naar Activity
                }
                catch (Exception)
                {
                    throw;
                }
            }
            else
            {
                permissionGranted = true;
            }
            return permissionGranted;
        }


        public async Task<string> Send(SmsDTO sms)
        {
            var smsMessenger = CrossMessaging.Current.SmsMessenger;
            string newStatus = "";

            bool hasPermission = await CheckPermissionAsync();

            if (hasPermission)
            {
                if (smsMessenger.CanSendSmsInBackground)
                {
                    try
                    {

                        if (IsSendEnabled)
                        {
                            smsMessenger.SendSmsInBackground(sms.ContactNumber, sms.Message);
                        }

                        newStatus = "Sent";
                        ErrorMessage = "";
                    }

                    catch (Exception ex)
                    {
                        newStatus = "Error";
                        ErrorMessage = ex.Message;
                    }
                }
                else
                {
                    newStatus = "Error";
                    ErrorMessage = "Cannot send in background";
                }
            }

            return newStatus;
        }


        public SendSmsService(bool canSend)
        {
            IsSendEnabled = canSend;
        }

        public SendSmsService() : this(false) { }
    }
}
