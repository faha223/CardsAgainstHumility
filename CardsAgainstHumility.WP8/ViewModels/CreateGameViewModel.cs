using CardsAgainstHumility.Helpers;
using CardsAgainstHumility.WP8.MVVM_Helpers;
using System;
using System.Windows.Input;
using System.Windows.Navigation;

namespace CardsAgainstHumility.WP8.ViewModels
{
    class CreateGameViewModel : ViewModelBase
    {
        private string _gameName = $"{CardsAgainstHumility.PlayerName}'s Game";
        public string GameName
        {
            get
            {
                return _gameName;
            }
            set
            {
                if (_gameName != value)
                {
                    _gameName = value;
                    OnPropertyChanged("GameName");
                }
            }
        }

        private int _maxPlayers = 10;
        public int MaxPlayers
        {
            get
            {
                return _maxPlayers;
            }
            set
            {
                if (_maxPlayers != value)
                {
                    _maxPlayers = value;
                    OnPropertyChanged("MaxPlayers");
                }
            }
        }

        private int _maxScore = 10;
        public int MaxScore
        {
            get
            {
                return _maxScore;
            }
            set
            {
                if (_maxScore != value)
                {
                    _maxScore = value;
                    OnPropertyChanged("MaxScore");
                }
            }
        }

        #region Start Game Command

        internal async void StartGame()
        {
            var gid = await CardsAgainstHumility.Add(CardsAgainstHumility.NewId(), GameName, MaxPlayers, MaxScore);
            await CardsAgainstHumility.JoinGame(gid);
            navService.Navigate(new Uri("/GamePage.xaml", UriKind.Relative));
        }

        private bool CanStartGame()
        {
            return true;
        }

        public ICommand StartGameCommand { get { return new ParameterlessCommandRouter(StartGame, CanStartGame); } }

        #endregion Start Game Command
    }
}
