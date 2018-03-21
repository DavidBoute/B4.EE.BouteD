using B4.EE.BouteD.Constants;
using B4.EE.BouteD.Models;
using B4.EE.BouteD.Services;
using B4.EE.BouteD.Services.Abstract;
using FreshMvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace B4.EE.BouteD.ViewModels
{
    public class SmsViewModel : FreshBasePageModel
    {
        private bool _IsFirstLoad = true;
        private ISmsDataService _smsDataService;
        private SignalRService _signalRService;
        private SendSmsService _sendSmsService;

        private bool _sendToggle;
        public bool SendToggle
        {
            get { return _sendToggle; }
            set { _sendToggle = value; RaisePropertyChanged(); }
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
                var smsToSendList = SmsList.Where(x => x.StatusName == "Pending");
                foreach (var smsToSend in smsToSendList)
                {
                    SendSmsCommand.Execute(smsToSend);
                }
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

                    if (smsSendresult == "Error")
                    {
                        await CoreMethods.DisplayAlert("Error", _sendSmsService.ErrorMessage, "OK");
                    }
                }
            });

        public ICommand GetSmsListCommand => new Command(
            async () =>
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

        // ReverseInit
        public override void ReverseInit(object value)
        {

        }

        // Init
        public override void Init(object initData)
        {

        }

        protected override async void ViewIsAppearing(object sender, EventArgs e)
        {
            // Bij eerste keer openen wat vertraging inbouwen bij aanvragen inladen,
            // anders wordt de Server Response niet opgevangen
            if (_IsFirstLoad)
            {
                await Task.Delay(1000);
                GetSmsListCommand.Execute(null);
                GetStatusListCommand.Execute(null);
                _IsFirstLoad = false;
            }
        }

        // Constructor
        public SmsViewModel(ISmsDataService dataService, SignalRService signalRService, SendSmsService sendSmsService)
        {
            SmsList = new ObservableCollection<SmsDTO>();
            ConnectionState = SignalRConnectionState.Closed;

            _smsDataService = dataService;
            _signalRService = signalRService;
            _sendSmsService = sendSmsService;

            // Events vanuit services
            #region Events Services

            MessagingCenter.Subscribe<List<SmsDTO>>(this, MessagingCenterConstants.SMS_LIST_GET,
                (smsDTOList) =>
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        SmsList = new ObservableCollection<SmsDTO>(smsDTOList);
                        IsRefreshing = false;
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
                    //Device.BeginInvokeOnMainThread(() =>
                    //{
                    var foundSms = SmsList.FirstOrDefault(x => x.Id == smsDTO.Id);
                    if (foundSms != null)
                    {
                        SendSmsCommand.Execute(foundSms);
                    }
                    //});
                });

            MessagingCenter.Subscribe<string>(this, MessagingCenterConstants.SMS_TOGGLE_SEND,
               (toggleMessage) =>
               {
                   SendSmsListCommand.Execute(null);
               });

            #endregion

            // ConnectionState updates
            #region ConnectionState updates
            MessagingCenter.Subscribe<SignalRService>(this, SignalRConnectionState.Open,
                (signalR) =>
                {
                    ConnectionState = SignalRConnectionState.Open;
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
