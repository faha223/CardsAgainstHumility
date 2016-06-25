using CardsAgainstHumility.GameClasses;
using CardsAgainstHumility.WP8.MVVM_Helpers;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Input;

namespace CardsAgainstHumility.WP8.ViewModels
{
    class LobbyViewModel : ViewModelBase
    {
        private List<GameInstance> _games;
        public List<GameInstance> Games
        {
            get
            {
                return _games;
            }
            set
            {
                if (_games != value)
                {
                    _games = value;
                    OnPropertyChanged("Games");
                }
            }
        }

        private string _status;
        public string Status
        {
            get
            {
                return _status;
            }
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged("Status");
                }
            }
        }


        public LobbyViewModel(): base()
        {
            var thd = new Thread(() =>
            {
                try
                {
                    var games = CardsAgainstHumility.ListAsync().Result;
                    dispatcher.BeginInvoke(() =>
                    {
                        Games = games;
                        Status = $"{(games.Count == 0 ? "No" : games.Count.ToString())} Games Found";
                    });
                }
                catch (Exception ex)
                {
                    dispatcher.BeginInvoke(() =>
                    {
                        Status = "Connection Error";
                    });
                }
                CardsAgainstHumility.Lobby_GameAdded += Lobby_GameAdded;
                CardsAgainstHumility.ConnectToLobby();
            });
            Status = "Refreshing";
            thd.Start();
        }

        private void Lobby_GameAdded(object sender, Events.GameAddedEventArgs e)
        {
            dispatcher.BeginInvoke(() =>
            {
                Games = e.Games;
            });
        }

        #region Join Game Command

        internal async void JoinGame(GameInstance game)
        {
            await CardsAgainstHumility.JoinGame(game.Id);
            navService.Navigate(new Uri("/GamePage.xaml", UriKind.Relative));
        }

        private bool CanJoinGame(GameInstance game)
        {
            return game.Players < game.MaxPlayers;
        }

        public ICommand JoinGameCommand { get { return new ParameteredCommandRouter<GameInstance>(JoinGame, CanJoinGame); } }

        #endregion Join Game Command
    }
}
