namespace CardsAgainstHumility.Interfaces
{
    public interface ISocketManager
    {
        ISocket GetSocket(string uri);

        ISocket GetSocket(string Uri, string query);
    }
}