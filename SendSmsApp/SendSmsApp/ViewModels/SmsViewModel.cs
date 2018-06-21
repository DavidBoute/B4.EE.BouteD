using SmsSenderApp.Constants;
using SmsSenderApp.Models;
using SmsSenderApp.Services;
using SmsSenderApp.Services.Abstract;
using FreshMvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace SmsSenderApp.ViewModels
{
    public class SmsViewModel : FreshBasePageModel
    {
        private ISmsDataService _smsDataService;
        private SignalRService _signalRService;
        private SendSmsService _sendSmsService;

        private bool _sendToggle;
        public bool SendToggle
        {
            get { return _sendToggle; }
            set { _sendToggle = value; RaisePropertyChanged(); }
        }

        private bool _sendLoopToggle;
        public bool SendLoopToggle
        {
            get { return _sendLoopToggle; }
            set
            {
                _sendLoopToggle = value;
                RaisePropertyChanged();
                if (value) { SendSmsListCommand.Execute(null); }
                _signalRService.NotifySendStatus(value);
            }
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

        public ICommand ToggleSendLoopCommand => new Command(
            () =>
            {
                SendLoopToggle = !SendLoopToggle;
            });

        public ICommand SendSmsListCommand => new Command(
            async () =>
            {
                while (_sendLoopToggle)
                {
                    var smsToSend = SmsList
                                        .Where(x => x.StatusName == "Pending")
                                        .OrderBy(x => x.TimeStamp)
                                        .FirstOrDefault(x => !x.IsSending);
                    if (smsToSend != null)
                    {
                        SendSmsCommand.Execute(smsToSend);
                    }

                    await Task.Delay(1000);
                };
            });

        public ICommand SendSmsCommand => new Command(
            async (smsDto) =>
            {
                SmsDTO sms = smsDto as SmsDTO;
                if (sms != null)
                {
                    string smsSendresult = await _sendSmsService
                                                    .CanSend(SendToggle)
                                                    .Send(sms);

                    StatusDTO newStatus = StatusList.SingleOrDefault(x => x.Name == smsSendresult);
                    if (newStatus != null)
                    {
                        sms.StatusId = newStatus.Id;
                        sms.StatusName = newStatus.Name;
                        _smsDataService.UpdateSms(sms);
                    }
                    sms.IsSending = false;

                    if (smsSendresult == "Error")
                    {
                        await CoreMethods.DisplayAlert("Error", _sendSmsService.ErrorMessage, "OK");
                    }
                }
            });

        public ICommand GetSmsListCommand => new Command(
            () =>
            {
                IsRefreshing = true;
                _smsDataService.GetSmsList();
            },
            () => { return !IsRefreshing; });

        public ICommand DeleteSmsCommand => new Command(
            (sms) =>
            {
                _smsDataService.DeleteSms(sms as SmsDTO);
            });

        public ICommand GetStatusListCommand => new Command(
           () =>
           {
               _smsDataService.GetStatusList();
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

                        _smsDataService.UpdateSms(sms);
                    }
                }
            });

        public ICommand ShowConnectionStateCommand => new Command(
            async () =>
            {
                await CoreMethods.DisplayAlert("Connectionstate...", ConnectionState, "OK");
            });

        // Constructor
        public SmsViewModel(ISmsDataService dataService, SignalRService signalRService, SendSmsService sendSmsService)
        {
            SmsList = new ObservableCollection<SmsDTO>();
            ConnectionState = SignalRConnectionState.Closed;

            _smsDataService = dataService;
            _signalRService = signalRService;
            _sendSmsService = sendSmsService;

            SendLoopToggle = false;

            // Events vanuit services
            #region Events Services

            MessagingCenter.Subscribe<List<SmsDTO>>(this, MessagingCenterConstants.SMS_LIST_GET,
                (smsDTOList) =>
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        SmsList = new ObservableCollection<SmsDTO>(smsDTOList);
                        IsRefreshing = false;

                        foreach (var smsDTO in SmsList)
                        {
                            // Status aanpassen naar Pending om aan te duiden dat de telefoon aan het verwerken is
                            if (smsDTO.StatusName == "Queued")
                            {
                                StatusDTO newStatus = StatusList.SingleOrDefault(x => x.Name == "Pending");
                                if (newStatus != null)
                                {
                                    smsDTO.StatusId = newStatus.Id;
                                    smsDTO.StatusName = newStatus.Name;
                                }

                                _smsDataService.UpdateSms(smsDTO);
                            }
                        }
                    });
                });

            MessagingCenter.Subscribe<List<StatusDTO>>(this, MessagingCenterConstants.STATUS_LIST_GET,
                (statusDTOList) =>
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        StatusList = statusDTOList;
                    });
                });

            MessagingCenter.Subscribe<SmsDTO>(this, MessagingCenterConstants.SMS_PUT,
                (smsDTO) =>
                {
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        var foundSms = SmsList.FirstOrDefault(x => x.Id == smsDTO.Id);
                        if (foundSms != null) // Als sms al bestaat: updaten
                        {
                            // Status aanpassen naar Pending om aan te duiden dat de telefoon aan het verwerken is
                            if (smsDTO.StatusName == "Queued")
                            {
                                StatusDTO newStatus = StatusList.SingleOrDefault(x => x.Name == "Pending");
                                if (newStatus != null)
                                {
                                    smsDTO.StatusId = newStatus.Id;
                                    smsDTO.StatusName = newStatus.Name;
                                }

                                _smsDataService.UpdateSms(smsDTO);
                            }

                            // Waarden sms in lijst aanpassen
                            // nieuwe verwijzing veroorzaakt fouten
                            if (!smsDTO.IsEqual(foundSms))
                            {
                                foundSms.CopyFrom(smsDTO);
                            }
                        }
                        else // Als hij nog niet bestaat: invoegen indien niet status Created
                        {
                            if (smsDTO.StatusName != "Created")
                            {
                                // Status aanpassen naar Pending om aan te duiden dat de telefoon aan het verwerken is
                                if (smsDTO.StatusName == "Queued")
                                {
                                    StatusDTO newStatus = StatusList.SingleOrDefault(x => x.Name == "Pending");
                                    if (newStatus != null)
                                    {
                                        smsDTO.StatusId = newStatus.Id;
                                        smsDTO.StatusName = newStatus.Name;
                                    }

                                    _smsDataService.UpdateSms(smsDTO);
                                }
                                else
                                {
                                    SmsList.Add(smsDTO);
                                }
                            }
                        }
                    });
                });

            MessagingCenter.Subscribe<SmsDTO>(this, MessagingCenterConstants.SMS_DELETE,
                (smsDTO) =>
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        var foundSms = SmsList.FirstOrDefault(x => x.Id == smsDTO.Id);
                        if (foundSms != null)
                        {
                            SmsList.Remove(foundSms);
                        }
                    });
                });

            MessagingCenter.Subscribe<SmsDTO>(this, MessagingCenterConstants.SMS_SEND,
                (smsDTO) =>
                {
                    var foundSms = SmsList.FirstOrDefault(x => x.Id == smsDTO.Id);
                    if (foundSms != null)
                    {
                        SendSmsCommand.Execute(foundSms);
                    }
                });

            MessagingCenter.Subscribe<string>(this, MessagingCenterConstants.SMS_TOGGLE_SEND,
               (toggleMessage) =>
               {
                   Device.BeginInvokeOnMainThread(() =>
                   {
                       bool.TryParse(toggleMessage, out bool sendLoop);
                       SendLoopToggle = sendLoop;
                   });
               });

            MessagingCenter.Subscribe<string>(this, MessagingCenterConstants.SMS_SENDSTATUS,
               (str) =>
               {
                   Device.BeginInvokeOnMainThread(() =>
                   {
                       signalRService.NotifySendStatus(SendLoopToggle);
                   });
               });

            #endregion

            // ConnectionState updates
            #region ConnectionState updates
            MessagingCenter.Subscribe<SignalRService>(this, SignalRConnectionState.Open,
                async(signalR) =>
                {
                    ConnectionState = SignalRConnectionState.Open;
                    GetStatusListCommand.Execute(null);
                    await Task.Delay(500);
                    GetSmsListCommand.Execute(null);

                });

            MessagingCenter.Subscribe<SignalRService>(this, SignalRConnectionState.Slow,
                (signalR) =>
                {
                    ConnectionState = SignalRConnectionState.Slow;
                });

            MessagingCenter.Subscribe<SignalRService>(this, SignalRConnectionState.Reconnecting,
                (signalR) =>
                {
                    ConnectionState = SignalRConnectionState.Reconnecting;
                });
            MessagingCenter.Subscribe<SignalRService>(this, SignalRConnectionState.Closed,
                (signalR) =>
                {
                    ConnectionState = SignalRConnectionState.Closed;
                });
            MessagingCenter.Subscribe<SignalRService>(this, SignalRConnectionState.Error,
                (signalR) =>
                {
                    ConnectionState = SignalRConnectionState.Error;
                });


            #endregion
        }

    }
}
