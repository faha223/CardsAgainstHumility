using System.Collections.Generic;

namespace CardsAgainstHumility.SocketComm
{
    public class GameState
    {
        public string name { get; set; }
        public string id { get; set; }
        public List<player> players { get; set; }
        public object[] history { get; set; }
        public bool isOver { get; set; }
        public string winnerId { get; set; }
        public string winningCard { get; set; }
        public bool isStarted { get; set; }
        public string currentBlackCard { get; set; }
        public bool isReadyForScoring { get; set; }
        public bool isReadyForReview { get; set; }
        public int pointsToWin { get; set; }
        public int maxPlayers { get; set; }
    }
}