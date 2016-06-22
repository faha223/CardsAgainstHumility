using System.Collections.Generic;

namespace CardsAgainstHumility.SocketComm
{
    public class player
    {
        public string id { get; set; }
        public string name { get; set; }
        public bool isReady { get; set; }
        public List<string> cards { get; set; }
        public string selectedWhiteCardId { get; set; }
        public int awesomePoints { get; set; }
        public bool isCzar { get; set; }
    }
}