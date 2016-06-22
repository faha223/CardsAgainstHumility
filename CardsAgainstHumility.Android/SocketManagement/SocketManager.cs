using CardsAgainstHumility.Interfaces;
using SocketManagement = CardsAgainstHumility.Android.SocketManagement;

namespace CardsAgainstHumility.Android.SocketManagement
{
    class SocketManager : ISocketManager
    {
        public ISocket GetSocket(string uri)
        {
            return new SocketManagement.Socket(uri);
        }

        public ISocket GetSocket(string uri, string query)
        {
            return new SocketManagement.Socket(uri, query);
        }
    }
}