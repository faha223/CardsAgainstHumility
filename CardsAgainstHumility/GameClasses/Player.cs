using CardsAgainstHumility.Helpers;

namespace CardsAgainstHumility.GameClasses
{
    public class Player : UINotifying
    {
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

        private int _awesomePoints;
        public int AwesomePoints
        {
            get
            {
                return _awesomePoints;
            }
            set
            {
                _awesomePoints = value;
                OnPropertyChanged("AwesomePoints");
            }
        }

        private bool _isCardCzar;
        public bool IsCardCzar
        {
            get
            {
                return _isCardCzar;
            }
            set
            {
                _isCardCzar = value;
                OnPropertyChanged("IsCardCzar");
            }
        }

        private bool _isReady;
        public bool IsReady
        {
            get
            {
                return _isReady;
            }
            set
            {
                _isReady = value;
                OnPropertyChanged("IsReady");
            }
        }
    }
}