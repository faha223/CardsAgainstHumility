using Newtonsoft.Json;

namespace CardsAgainstHumility.SocketComm
{
    public class connectToGameArgs
    {
        public string gameId { get; set; }
        public string playerId { get; set; }
        public string playerName { get; set; }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}