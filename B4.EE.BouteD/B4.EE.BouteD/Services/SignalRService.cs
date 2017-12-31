using B4.EE.BouteD.Models;
using FreshMvvm;
using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace B4.EE.BouteD.Services
{
    public class SignalRService
    {
        private FreshBasePageModel _pageModel;

        private HubConnection _hubConnection;
        private IHubProxy _chatHubProxy;

        // Methods to call on the server
        public void Send(string name, string message)
        {
            // Invoke the 'Send' method on the server
            _chatHubProxy.Invoke("Send", name, message);
        }

        async void StartConnection()
        {
            // Connect to the server
            _hubConnection = new HubConnection($"{ConnectionSettings.Prefix}{ConnectionSettings.Host}:{ConnectionSettings.Port}");

            // Create a proxy to the 'ServerSentEventsHub' SignalR Hub
            _chatHubProxy = _hubConnection.CreateHubProxy("ServerSentEventsHub");

            // Add EventHandlers
            AddEventHandlers();

            // Start the connection
            await _hubConnection.Start();
        }

        void AddEventHandlers()
        {
            _chatHubProxy.On<SmsDTOWithClient>("notifyChangeToPage", smsDTOWithClient =>
                    MessagingCenter.Send(smsDTOWithClient, smsDTOWithClient.Operation));

            //_chatHubProxy.On<SmsDTOWithClient>("notifyChangeToPage", smsDTOWithClient =>
            //       _pageModel.CoreMethods.DisplayAlert("New message from server", string.Format("{0}: {1} \r\n", smsDTOWithClient.Client, smsDTOWithClient.Operation), "OK"));



            _chatHubProxy.On<string>("addNewMessageToPage", message =>
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
