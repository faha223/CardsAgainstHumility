using System;

namespace CardsAgainstHumility.Events
{
    public sealed class SocketConnectErrorEventArgs : EventArgs
    {
        public string Message { get; private set; }
        public SocketConnectErrorEventArgs(string message)
        {
            Message = message;
        }
    }
}