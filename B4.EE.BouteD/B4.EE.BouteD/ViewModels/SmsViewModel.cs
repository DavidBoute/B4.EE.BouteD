using B4.EE.BouteD.Constants;
using B4.EE.BouteD.Models;
using B4.EE.BouteD.Services;
using FreshMvvm;
using Plugin.Messaging;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Xamarin.Forms;

namespace B4.EE.BouteD.ViewModels
{
    public class SmsViewModel : FreshBasePageModel
    {
        private SmsFromRestService _smsService;
        private SignalRService _signalRService;

        private bool _canSend;
        public bool CanSend
        {
            get { return _canSend; }
            set { _canSend = value; RaisePropertyChanged(); }
        }

        private bool _isRefreshing;
        public bool IsRefreshing
        {
            get { return _isRefreshing; }
            set { _isRefreshing = value; RaisePropertyChanged(); }
        }

        private string _connectionState;
        public string ConnectionState
        {
            get { return _connectionState; }
            set { _connectionState = value; RaisePropertyChanged(); }
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
            set { _smsList = value; RaisePropertyChanged(); }
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
            async (smsDto) =>
            {
                var sms = smsDto as SmsDTO;
                if (sms != null)
                {
                    var smsMessenger = CrossMessaging.Current.SmsMessenger;
                    StatusDTO newStatus;
                    if (smsMessenger.CanSendSmsInBackground)
                    {
                        try
                        {
                            if (CanSend)
                            {

                                try
                                {
                                    var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Sms);
                                    if (status != PermissionStatus.Granted)
                                    {
                                        var results = await CrossPermissions.Current.RequestPermissionsAsync(Permission.Sms);
                                        //Best practice to always check that the key exists
                                        if (results.ContainsKey(Permission.Sms))
                                            status = results[Permission.Sms];
                                    }

                                    if (status == PermissionStatus.Granted)
                                    {
                                        smsMessenger.SendSmsInBackground("+32494240152", "Well hello there from Xam.Messaging.Plugin");
                                    }
                                    else if (status != PermissionStatus.Unknown)
                                    {
                                        await CoreMethods.DisplayAlert("Sms toestemming geweigerd", "Probeer later opnieuw.", "OK");
                                    }
                                }
                                catch (Exception)
                                {
                                }

                            }
                            else
                                await CoreMethods.DisplayAlert("Send SMS success", sms.ContactFullName + " - " + sms.Message, "Cancel");

                            newStatus = StatusList.SingleOrDefault(x => x.Name == "Sent");
                        }
                        catch (Exception)
                        {
                            await CoreMethods.DisplayAlert("Send SMS failure", "Kan SMS niet verzenden", "Cancel");

                            newStatus = StatusList.SingleOrDefault(x => x.Name == "Error");
                        }
                    }
                    else
                    {
                        await CoreMethods.DisplayAlert("Send SMS failure", "SMS niet mogelijk", "Cancel");

                        newStatus = StatusList.SingleOrDefault(x => x.Name == "Error");
                    }
                    if (newStatus != null)
                    {
                        sms.StatusId = newStatus.Id;
                        sms.StatusName = newStatus.Name;
                    }

                    string res = await _smsService.UpdateSms(sms);
                    if (res == "OK")
                    {
                        _signalRService.NotifyChange(new SmsDTOWithClient
                        {
                            Operation = "PUT",
                            Client = "Xamarin",
                            SmsDTO = sms
                        });
                    }

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

                IsRefreshing = false;
            },
            () => { return !IsRefreshing; });

        public ICommand DeleteSmsCommand => new Command(
            async (guid) =>
            {
                string res = await _smsService.DeleteSms(guid as string);
                if (res == "OK")
                {
                    SmsDTO smsToDelete = SmsList.SingleOrDefault(x => x.Id == guid as string);

                    _signalRService.NotifyChange(new SmsDTOWithClient
                    {
                        Operation = "DELETE",
                        Client = "Xamarin",
                        SmsDTO = smsToDelete
                    });

                    SmsList.Remove(smsToDelete);
                }
            });

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

                        string res = await _smsService.UpdateSms(sms);
                        if (res == "OK")
                        {
                            _signalRService.NotifyChange(new SmsDTOWithClient
                            {
                                Operation = "PUT",
                                Client = "Xamarin",
                                SmsDTO = sms
                            });
                        }
                    }
                }
            });

        public ICommand ShowConnectionStateCommand => new Command(
            async () =>
            {
                await CoreMethods.DisplayAlert("Connectionstate...", ConnectionState, "OK");
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
            ConnectionState = SignalRConnectionState.Closed;

            _smsService = new SmsFromRestService();
            _signalRService = new SignalRService(this);

            // Server Sent Events
            #region Server Sent Events
            MessagingCenter.Subscribe<SmsDTOWithClient>(this, "PUT",
                (smsDTOWithClient) =>
                {
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        var foundSms = SmsList.FirstOrDefault(x => x.Id == smsDTOWithClient.SmsDTO.Id);
                        if (foundSms != null) // Als sms al bestaat: updaten
                        {
                            // Status aanpassen naar Pending om aan te duiden dat de telefoon aan het verwerken is
                            if (smsDTOWithClient.SmsDTO.StatusName == "Queued")
                            {
                                StatusDTO newStatus = StatusList.SingleOrDefault(x => x.Name == "Pending");
                                if (newStatus != null)
                                {
                                    smsDTOWithClient.SmsDTO.StatusId = newStatus.Id;
                                    smsDTOWithClient.SmsDTO.StatusName = newStatus.Name;
                                }

                                string res = await _smsService.UpdateSms(smsDTOWithClient.SmsDTO);
                                if (res == "OK")
                                {
                                    _signalRService.NotifyChange(new SmsDTOWithClient
                                    {
                                        Operation = "PUT",
                                        Client = "Xamarin",
                                        SmsDTO = smsDTOWithClient.SmsDTO
                                    });
                                }
                            }

                            if (!smsDTOWithClient.SmsDTO.IsEqual(foundSms))
                            {
                                foundSms.CopyFrom(smsDTOWithClient.SmsDTO);
                            }
                        }
                        else // Als hij nog niet bestaat: invoegen indien niet status Created
                        {
                            if (smsDTOWithClient.SmsDTO.StatusName != "Created")
                            {
                                SmsList.Add(smsDTOWithClient.SmsDTO);
                            }
                        }
                    });
                });

            MessagingCenter.Subscribe<SmsDTOWithClient>(this, "DELETE",
                (smsDTOWithClient) =>
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        var foundSms = SmsList.FirstOrDefault(x => x.Id == smsDTOWithClient.SmsDTO.Id);
                        if (foundSms != null)
                        {
                            SmsList.Remove(foundSms);
                        }
                    });
                });
            #endregion

            // ConnectionState updates
            #region ConnectionState updates
            MessagingCenter.Subscribe<SignalRService>(this, SignalRConnectionState.Open,
                (signalRService) =>
                {
                    ConnectionState = SignalRConnectionState.Open;
                });

            MessagingCenter.Subscribe<SignalRService>(this, SignalRConnectionState.Slow,
                (signalRService) =>
                {
                    ConnectionState = SignalRConnectionState.Slow;
                });

            MessagingCenter.Subscribe<SignalRService>(this, SignalRConnectionState.Reconnecting,
                (signalRService) =>
                {
                    ConnectionState = SignalRConnectionState.Reconnecting;
                });
            MessagingCenter.Subscribe<SignalRService>(this, SignalRConnectionState.Closed,
                (signalRService) =>
                {
                    ConnectionState = SignalRConnectionState.Closed;
                });
            MessagingCenter.Subscribe<SignalRService>(this, SignalRConnectionState.Error,
                (signalRService) =>
                {
                    ConnectionState = SignalRConnectionState.Error;
                });


            #endregion
        }

    }
}
