using B4.EE.BouteD.Models;
using FreshMvvm;
using Microsoft.AspNet.SignalR.Client;
using System.Collections.Generic;
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

            // Add EventHandlers
            AddEventHandlers();

            // Start the connection
            await _hubConnection.Start();
        }

        void AddEventHandlers()
        {
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
