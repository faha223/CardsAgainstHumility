using CardsAgainstHumility.Interfaces;
using System;
using SocketIO.Client;

namespace CardsAgainstHumility.Android.SocketManagement
{
    class Socket : ISocket
    {
        SocketIO.Client.Socket sock;

        public Socket(string path)
        {
            Init(path, null);
        }

        public Socket(string path, string query)
        {
            Init(path, query);
        }

        private void Init(string path, string query)
        {
            if (query != null)
            {
                var opts = new IO.Options()
                {
                    Query = query
                };
                sock = IO.Socket(path, opts);
            }
            else
                sock = IO.Socket(path);
        }

        public void Connect()
        {
            sock.Connect();
        }

        public void On(string eventName, Action<object[]> listener)
        {
            sock.On(eventName, listener);
        }

        public void Emit(string message)
        {
            sock.Emit(message);
        }

        public void Emit(string message, string messageBody)
        {
            sock.Emit(message, messageBody);
        }

        public void Close()
        {
            sock.Close();
        }
    }
}