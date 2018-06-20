using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmsSenderApp.Constants
{
    public class SignalRConnectionState
    {
        public const string Slow = "CONNECTION_SLOW";
        public const string Reconnecting = "CONNECTION_RECONNECTING";
        public const string Closed = "CONNECTION_CLOSED";
        public const string Open = "CONNECTION_OPEN";
        public const string Error = "CONNECTION_ERROR";
    }
}
