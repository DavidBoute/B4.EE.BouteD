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

                        SmsFromRestService smsService = new SmsFromRestService();
                        smsService.UpdateSms(sms);
                    }
                    else
                        CoreMethods.DisplayAlert("Send SMS failure", "Kan SMS niet verzenden", "Cancel");
                }
            });

        public ICommand GetSmsListCommand => new Command(
            async () =>
            {
                SmsFromRestService smsService = new SmsFromRestService();
                if (SmsList.Count != 0)
                {
                    var newList = await smsService.GetSmsList();

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
                    SmsList = await smsService.GetSmsList();
                }
            });

        public ICommand DeleteSmsCommand => new Command(
            async (guid) =>
            {
                SmsFromRestService smsService = new SmsFromRestService();
                string res = await smsService.DeleteSms(guid as string);
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
               SmsFromRestService smsService = new SmsFromRestService();
               StatusList = await smsService.GetStatusList();
           });

        public ICommand OpenSettingsCommand => new Command(
            async () =>
            {
                await CoreMethods.PushPageModel<SettingsViewModel>(true);
            });

        // Constructor
        public SmsViewModel()
        {
            SmsList = new ObservableCollection<SmsDTO>();
            AutoUpdate = false;
            GetSmsListCommand.Execute(null);
            GetStatusListCommand.Execute(null);
        }

    }
}
