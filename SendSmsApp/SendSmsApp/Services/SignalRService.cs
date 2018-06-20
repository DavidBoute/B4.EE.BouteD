
using SmsSenderApp.Constants;
using SmsSenderApp.Models;
using FreshMvvm;
using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SmsSenderApp.Services
{
    public class SignalRService
    {
        private HubConnection _hubConnection;
        private IHubProxy _hubProxy;

        // Methods to invoke on the server
        #region Server Methods

        public void Send(string message)
        {
            Task request = new Task(() => _hubProxy.Invoke("Send", message));
            InvokeServerMethod(request);
        }

        public void NotifyChange(SmsDTOWithOperation smsDTOWithOperation)
        {
            Task request = new Task(() => _hubProxy.Invoke("NotifyChange", smsDTOWithOperation));
            InvokeServerMethod(request);
        }

        public void NotifySendStatus(bool IsSending)
        {
            Task request = new Task(() => _hubProxy.Invoke("NotifySendStatus", IsSending));
            InvokeServerMethod(request);
        }

        public void RequestSmsList()
        {
            Task request = new Task(() => _hubProxy.Invoke("RequestSmsList", false)); // parameter includeCreated
            InvokeServerMethod(request);
        }

        public void RequestStatusList()
        {
            Task request = new Task(() => _hubProxy.Invoke("RequestStatusList"));
            InvokeServerMethod(request);
        }

        public void RequestUpdateSms(SmsDTO smsDTO)
        {
            Task request = new Task(() => _hubProxy.Invoke("RequestEditSms", smsDTO));
            InvokeServerMethod(request);
        }

        public void RequestDeleteSms(SmsDTO smsDTO)
        {
            Task request = new Task(() => _hubProxy.Invoke("RequestDeleteSms", smsDTO));
            InvokeServerMethod(request);
        }

        public async void InvokeServerMethod(Task request)
        {
            if (_hubConnection.State == ConnectionState.Connected)
            {
                request.Start();
            }
            else if (_hubConnection.State == ConnectionState.Connecting
                    || _hubConnection.State == ConnectionState.Reconnecting)
            {
                await Task.Delay(500);
                InvokeServerMethod(request);
            }
        }

        #endregion

        // LifeTime Event Handlers
        #region LifeTime Event Handlers

        async void OnStateChanged(StateChange stateChange)
        {
            if (stateChange.NewState == ConnectionState.Connected)
                OnOpened();
        }

        async void OnOpened()
        {
            Debug.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ": Signal R Connection open");
            MessagingCenter.Send(this, SignalRConnectionState.Open);
        }

        async void OnConnectionSlow()
        {
            Debug.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ": Signal R Connection slow");
            MessagingCenter.Send(this, SignalRConnectionState.Slow);
        }

        async void OnReconnecting()
        {
            Debug.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ": Signal R Connection reconnecting");
            MessagingCenter.Send(this, SignalRConnectionState.Reconnecting);
        }

        async void OnClosed()
        {
            Debug.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ": Signal R Connection closed");
            MessagingCenter.Send(this, SignalRConnectionState.Closed);

            await Task.Delay(5000);

            try
            {
                await _hubConnection.Start();
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        async void OnError(Exception ex)
        {
            Debug.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ": Signal R - " + ex.Message);
            MessagingCenter.Send(this, SignalRConnectionState.Error);

            if (_hubConnection.State == ConnectionState.Disconnected)
            {
                OnClosed();
            }
        }

        #endregion

        void AddEventHandlers()
        {
            // Signal R Lifetime Events

            _hubConnection.StateChanged += OnStateChanged;
            _hubConnection.ConnectionSlow += OnConnectionSlow;
            _hubConnection.Reconnecting += OnReconnecting;
            _hubConnection.Closed += OnClosed;
            _hubConnection.Error += OnError;

            // Server Sent Events

            _hubProxy.On("notifyChangeToSmsList", () =>
                {
                    RequestSmsList();
                });

            _hubProxy.On<string>("DisplayMessage", message =>
                MessagingCenter.Send(message, MessagingCenterConstants.MESSAGE));

            _hubProxy.On<List<SmsDTO>>("GetSmsList", smsList =>
                MessagingCenter.Send(smsList, MessagingCenterConstants.SMS_LIST_GET));

            _hubProxy.On<List<StatusDTO>>("GetStatusList", statusList =>
                MessagingCenter.Send(statusList, MessagingCenterConstants.STATUS_LIST_GET));

            _hubProxy.On<SmsDTO>("NotifyCreateSms", smsDTO =>
                MessagingCenter.Send(smsDTO, MessagingCenterConstants.SMS_POST));

            _hubProxy.On<SmsDTO>("NotifyEditSms", smsDTO =>
                MessagingCenter.Send(smsDTO, MessagingCenterConstants.SMS_PUT));

            _hubProxy.On<SmsDTO>("NotifyDeleteSms", smsDTO =>
                MessagingCenter.Send(smsDTO, MessagingCenterConstants.SMS_DELETE));

            _hubProxy.On<SmsDTO>("SendSelectedSms", smsDTO =>
                MessagingCenter.Send(smsDTO, MessagingCenterConstants.SMS_SEND));

            _hubProxy.On<bool>("ToggleSendPending", toggle =>
                MessagingCenter.Send(toggle.ToString(), MessagingCenterConstants.SMS_TOGGLE_SEND));

            _hubProxy.On("GetSendStatus", () =>
                MessagingCenter.Send("SendStatus",MessagingCenterConstants.SMS_TOGGLE_SEND));
        }

        async void StartConnection(ConnectionSettings connectionSettings)
        {
            // Connect to the server
            _hubConnection = new HubConnection($"{connectionSettings.Prefix}{connectionSettings.Host}:{connectionSettings.Port}");

            // Create a proxy to the 'ServerSentEventsHub' SignalR Hub
            _hubProxy = _hubConnection.CreateHubProxy("ServerSentEventsHub");

            // Start the connection
            try
            {
                await _hubConnection.Start();
                OnOpened();
            }
            catch (Exception ex)
            {
                OnError(ex);
            }

            // Add EventHandlers
            AddEventHandlers();
        }

        // Singleton implementation
        #region Singleton implementation

        private static SignalRService _instance
            = new SignalRService(
                    FreshIOC.Container.Resolve<ConnectionSettings>());

        public static SignalRService Instance()
        {
            if (_instance == null)
            {
                _instance = new SignalRService(
                    FreshIOC.Container.Resolve<ConnectionSettings>());
            }

            return _instance;
        }

        // private constructor
        private SignalRService(ConnectionSettings connectionSettings)
        {
            StartConnection(connectionSettings);

            MessagingCenter.Subscribe<ConnectionSettings>(this, "UpdateConnectionSettings",
                (connection) =>
                {
                    StartConnection(connection);
                });
        }

        #endregion
    }
}
