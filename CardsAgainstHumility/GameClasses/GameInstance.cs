using CardsAgainstHumility.Helpers;

namespace CardsAgainstHumility.GameClasses
{
    public class GameInstance : UINotifying
    {
        private string _id;
        public string Id
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
                OnPropertyChanged("Id");
            }
        }

        private string _name;
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                OnPropertyChanged("Name");
            }
        }

        private int _players;
        public int Players
        {
            get
            {
                return _players;
            }
            set
            {
                _players = value;
                OnPropertyChanged("Players");
            }
        }

        private int _maxPlayers;
        public int MaxPlayers
        {
            get
            {
                return _maxPlayers;
            }
            set
            {
                _maxPlayers = value;
                OnPropertyChanged("MaxPlayers");
            }
        }
    }
}