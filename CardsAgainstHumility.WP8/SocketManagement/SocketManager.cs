using CardsAgainstHumility.Interfaces;
using Quobject.SocketIoClientDotNet.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardsAgainstHumility.WP8.SocketManagement
{
    class SocketManager : ISocketManager
    {
        public ISocket GetSocket(string uri)
        {
            var sock = IO.Socket(uri);
            return new Socket(sock);
        }

        public ISocket GetSocket(string uri, string query)
        {
            var sock = IO.Socket($"{uri}?{query}");
            return new Socket(sock);
        }
    }
}
