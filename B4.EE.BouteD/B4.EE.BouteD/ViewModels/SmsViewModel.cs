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
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace B4.EE.BouteD.ViewModels
{
    public class SmsViewModel : FreshBasePageModel
    {
        private bool _IsFirstLoad = true;
        private SmsFromRestService _smsRestService;
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
                SmsDTO sms = smsDto as SmsDTO;
                if (sms != null)
                {
                    SendSmsService smsSendService = new SendSmsService(CanSend);
                    string smsSendresult = await smsSendService.Send(sms);

                    StatusDTO newStatus = StatusList.SingleOrDefault(x => x.Name == smsSendresult);
                    if (newStatus != null)
                    {
                        sms.StatusId = newStatus.Id;
                        sms.StatusName = newStatus.Name;
                        _smsRestService.UpdateSms(sms);
                    } 
                }
            });

        public ICommand GetSmsListCommand => new Command(
            async () =>
            {
                IsRefreshing = true;
                _signalRService.RequestSmsList();
            },
            () => { return !IsRefreshing; });

        public ICommand DeleteSmsCommand => new Command(
            (sms) =>
            {
                _smsRestService.DeleteSms(sms as SmsDTO);
            });

        public ICommand GetStatusListCommand => new Command(
           () =>
           {
               _smsRestService.GetStatusList();
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

                        _smsRestService.UpdateSms(sms);
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
            if (_IsFirstLoad)
            {
                await Task.Delay(1000);
                GetSmsListCommand.Execute(null);
                GetStatusListCommand.Execute(null);
                _IsFirstLoad = false;
            }
        }

        // Constructor
        public SmsViewModel()
        {
            SmsList = new ObservableCollection<SmsDTO>();
            ConnectionState = SignalRConnectionState.Closed;

            _smsRestService = SmsFromRestService.Instance();
            _signalRService = SignalRService.Instance();

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

                                _smsRestService.UpdateSms(smsDTO);
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
                                SmsList.Add(smsDTO);
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
