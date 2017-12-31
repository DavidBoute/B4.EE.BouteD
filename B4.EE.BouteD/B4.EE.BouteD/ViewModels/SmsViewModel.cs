using B4.EE.BouteD.Models;
using B4.EE.BouteD.Services;
using FreshMvvm;
using Newtonsoft.Json;
using Plugin.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Xamarin.Forms;
using B4.EE.BouteD.Pages;

namespace B4.EE.BouteD.ViewModels
{
    public class SmsViewModel : FreshBasePageModel
    {
        private SmsFromRestService _smsService;
        private SignalRService _signalRService;

        private bool _autoUpdate;
        public bool AutoUpdate
        {
            get { return _autoUpdate; }
            set
            {
                _autoUpdate = value;
                RaisePropertyChanged();
                ToggleAutoUpdate();
            }
        }

        private ICollection<StatusDTO> _statusList;
        public ICollection<StatusDTO> StatusList
        {
            get { return _statusList; }
            set { _statusList = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<SmsDTO> _smsList;
        public ObservableCollection<SmsDTO> SmsList
        {
            get { return _smsList; }
            set
            { _smsList = value; RaisePropertyChanged(); }
        }

        public ICommand SendSmsListCommand => new Command(
            () =>
            {
                var smsToSendList = SmsList.Where(x => true);
                foreach (var smsToSend in smsToSendList)
                {
                    SendSmsCommand.Execute(smsToSend);
                }
            });

        public ICommand SendSmsCommand => new Command(
            (smsDto) =>
            {
                var sms = smsDto as SmsDTO;
                if (sms != null)
                {
                    var smsMessenger = CrossMessaging.Current.SmsMessenger;
                    if (smsMessenger.CanSendSmsInBackground)
                    {
                        //smsMessenger.SendSmsInBackground("+32494240152", "Well hello there from Xam.Messaging.Plugin");
                        CoreMethods.DisplayAlert("Send SMS success", sms.ContactFullName + " - " + sms.Message, "Cancel");

                        StatusDTO newStatus = StatusList.SingleOrDefault(x => x.Name == "Sent");
                        if (newStatus != null)
                        {
                            sms.StatusId = newStatus.Id;
                            sms.StatusName = newStatus.Name;
                        }

                        _smsService.UpdateSms(sms);
                    }
                    else
                        CoreMethods.DisplayAlert("Send SMS failure", "Kan SMS niet verzenden", "Cancel");
                }
            });

        public ICommand GetSmsListCommand => new Command(
            async () =>
            {
                if (SmsList.Count != 0)
                {
                    var newList = await _smsService.GetSmsList();

                    // Elementen toevoegen/aanpassen
                    foreach (SmsDTO sms in newList)
                    {
                        var foundSms = SmsList.FirstOrDefault(x => x.Id == sms.Id);
                        if (foundSms != null) // Update
                        {
                            if (!sms.IsEqual(foundSms))
                            {
                                int i = SmsList.IndexOf(foundSms);
                                SmsList[i] = sms;
                            }
                        }
                        else // Insert
                        {
                            SmsList.Add(sms);
                        }
                    }

                    // Elementen verwijderen
                    for (int i = SmsList.Count - 1; i >= 0; i--)
                    {
                        var sms = SmsList.ElementAt(i);
                        var foundSms = newList.FirstOrDefault(x => x.Id == sms.Id);
                        if (foundSms == null) // Delete
                        {
                            SmsList.Remove(sms);
                        }
                    }
                }
                else
                {
                    SmsList = await _smsService.GetSmsList();
                }
            });

        public ICommand DeleteSmsCommand => new Command(
            async (guid) =>
            {
                string res = await _smsService.DeleteSms(guid as string);
            });

        private void ToggleAutoUpdate()
        {
            if (AutoUpdate)
            {
                Device.StartTimer(new TimeSpan(0, 0, 0, 5), () =>
                    {
                        GetSmsListCommand.Execute(null);
                        return AutoUpdate;
                    }
                );
            }
        }

        public ICommand GetStatusListCommand => new Command(
           async () =>
           {
               StatusList = await _smsService.GetStatusList();
           });

        public ICommand OpenSettingsCommand => new Command(
            async () =>
            {
                await CoreMethods.PushPageModel<SettingsViewModel>(true);
            });


        public ICommand ChangeSmsStatusCommand => new Command(
            async (smsDto) =>
            {
                var sms = smsDto as SmsDTO;
                if (sms != null)
                {
                    string result = await CoreMethods.DisplayActionSheet("Select new status", "Ok", "Cancel",
                        StatusList.Select(x => x.Name).ToArray());

                    if (result != sms.StatusName)
                    {
                        StatusDTO newStatus = StatusList.SingleOrDefault(x => x.Name == result);
                        if (newStatus != null)
                        {
                            sms.StatusId = newStatus.Id;
                            sms.StatusName = newStatus.Name;
                        }

                        _smsService.UpdateSms(sms);
                    }
                }
            });

        // ReverseInit
        public override void ReverseInit(object value)
        {

        }

        // Init
        public override void Init(object initData)
        {
            GetSmsListCommand.Execute(null);
            GetStatusListCommand.Execute(null);
        }

        // Constructor
        public SmsViewModel()
        {
            SmsList = new ObservableCollection<SmsDTO>();
            AutoUpdate = false;

            _smsService = new SmsFromRestService();
            _signalRService = new SignalRService(this);

            // Server Sent Events
            MessagingCenter.Subscribe<SmsDTOWithClient>(this, "POST", (smsDTOWithClient) =>
                 {
                     CoreMethods.DisplayAlert("New message from server", smsDTOWithClient.Operation,"OK");
                 });

            MessagingCenter.Subscribe<SmsDTOWithClient>(this, "PUT", (smsDTOWithClient) =>
            {
                CoreMethods.DisplayAlert("New message from server", smsDTOWithClient.Operation, "OK");
            });

            MessagingCenter.Subscribe<SmsDTOWithClient>(this, "DELETE", (smsDTOWithClient) =>
            {
                CoreMethods.DisplayAlert("New message from server", smsDTOWithClient.Operation, "OK");
            });

        }

    }
}
