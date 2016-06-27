using CardsAgainstHumility.Interfaces;
using Quobject.SocketIoClientDotNet.Client;

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
            var opts = new IO.Options();
            opts.QueryString = query;
            var sock = IO.Socket($"{uri}", opts);
            return new Socket(sock);
        }
    }
}
