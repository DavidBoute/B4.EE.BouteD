using B4.EE.BouteD.Constants;
using B4.EE.BouteD.Models;
using FreshMvvm;
using Microsoft.AspNet.SignalR.Client;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace B4.EE.BouteD.Services
{
    public class SignalRService
    {
        private FreshBasePageModel _pageModel;

        private HubConnection _hubConnection;
        private IHubProxy _hubProxy;

        // Methods to call on the server
        public void Send(string name, string message)
        {
            // Invoke the 'Send' method on the server
            _hubProxy.Invoke("Send", name, message);
        }

        public void NotifyChange(SmsDTOWithClient smsDTOWithClient)
        {
            // Invoke the 'Send' method on the server
            _hubProxy.Invoke("NotifyChange", smsDTOWithClient);
        }

        async void StartConnection()
        {
            // Connect to the server
            _hubConnection = new HubConnection($"{ConnectionSettings.Prefix}{ConnectionSettings.Host}:{ConnectionSettings.Port}");

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

        // EventHandlers
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
            Debug.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") +": Signal R Connection slow");
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

        void AddEventHandlers()
        {
            // Signal R Lifetime Events

            _hubConnection.StateChanged += OnStateChanged;
            _hubConnection.ConnectionSlow += OnConnectionSlow;
            _hubConnection.Reconnecting += OnReconnecting;
            _hubConnection.Closed += OnClosed;
            _hubConnection.Error += OnError;
            
            // Events voor berichten
            _hubProxy.On<SmsDTOWithClient>("notifyChangeToPage", smsDTOWithClient =>
                  MessagingCenter.Send(smsDTOWithClient, smsDTOWithClient.Operation));

            _hubProxy.On<string>("addNewMessageToPage", message =>
                    _pageModel.CoreMethods.DisplayAlert("New message from server", string.Format("Received Msg: {0}\r\n", message), "OK"));
        }

        // Constructor
        public SignalRService(FreshBasePageModel pageModel)
        {
            _pageModel = pageModel;
            StartConnection();
        }
    }
}
