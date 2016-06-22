using CardsAgainstHumility.Helpers;

namespace CardsAgainstHumility.GameClasses
{
    public class GameInstance : UINotifying
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Players { get; set; }
        public int MaxPlayers { get; set; }
    }
}