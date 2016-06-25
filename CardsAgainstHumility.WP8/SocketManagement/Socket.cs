using CardsAgainstHumility.Interfaces;
using System;

namespace CardsAgainstHumility.WP8.SocketManagement
{
    class Socket : ISocket
    {
        private Quobject.SocketIoClientDotNet.Client.Socket sock;

        public Socket(Quobject.SocketIoClientDotNet.Client.Socket socket)
        {
            sock = socket;
        }

        public void Close()
        {
            sock.Close();
        }

        public void Connect()
        {
            sock.Connect();
        }

        public void Emit(string message)
        {
            sock.Emit(message);
        }

        public void Emit(string message, string messageBody)
        {
            sock.Emit(message, messageBody);
        }

        public void On(string eventName, Action<object[]> listener)
        {
            sock.On(eventName, (data) =>
            {
                listener.Invoke(new object[] { data });
            });
        }
    }
}
