﻿using CardsAgainstHumility.WP8.MVVM_Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Input;

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

        private List<SelectableItem> _decks;
        public List<SelectableItem> Decks
        {
            get
            {
                return _decks;
            }
            set
            {
                if (_decks != value)
                {
                    _decks = value;
                    OnPropertyChanged("Decks");
                }
            }
        }

        #region Start Game Command

        internal async void StartGame()
        {
            List<string> decks = Decks.Where(c => c.IsSelected).Select(c => c.Text).ToList();
            var gid = await CardsAgainstHumility.Add(CardsAgainstHumility.NewId(), GameName, decks, MaxPlayers, MaxScore);
            await CardsAgainstHumility.JoinGame(gid);
            navService.Navigate(new Uri("/GamePage.xaml", UriKind.Relative));
        }

        private bool CanStartGame()
        {
            return true;
        }

        public ICommand StartGameCommand { get { return new ParameterlessCommandRouter(StartGame, CanStartGame); } }

        #endregion Start Game Command

        public CreateGameViewModel() : base()
        {
            Decks = new List<SelectableItem>(0);
            Thread thd = new Thread(async () =>
            {
                List<string> deckNames = await CardsAgainstHumility.GetDecks();
                dispatcher.BeginInvoke(() =>
                {
                    try
                    {
                        Decks = deckNames.Select(c => new SelectableItem()
                        {
                            IsSelected = true,
                            Text = c
                        }).ToList();
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                });
            });
            thd.Start();
        }
    }
}
