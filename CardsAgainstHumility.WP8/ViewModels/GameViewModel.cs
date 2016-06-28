using CardsAgainstHumility.GameClasses;
using CardsAgainstHumility.WP8.MVVM_Helpers;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Linq;
using CardsAgainstHumility.Events;
using System.Collections.Generic;
using System;

namespace CardsAgainstHumility.WP8.ViewModels
{
    class GameViewModel : ViewModelBase
    {
        #region Properties

        private WhiteCard selectedWhiteCard;
        public WhiteCard SelectedWhiteCard
        {
            get
            {
                return selectedWhiteCard;
            }
            set
            {
                if (selectedWhiteCard != value)
                {
                    selectedWhiteCard = value;
                    OnPropertyChanged("SelectedWhiteCard");
                    OnPropertyChanged("ConfirmCommand");
                }
            }
        }

        private WhiteCard confirmedWhiteCard;
        public WhiteCard ConfirmedWhiteCard
        {
            get
            {
                return confirmedWhiteCard;
            }
            set
            {
                confirmedWhiteCard = value;
                OnPropertyChanged("ConfirmedWhiteCard");
            }
        }

        private BlackCard currentQuestion;
        public BlackCard CurrentQuestion
        {
            get
            {
                return currentQuestion;
            }
            set
            {
                if (currentQuestion != value)
                {
                    currentQuestion = value;
                    OnPropertyChanged("CurrentQuestion");
                }
            }
        }

        private int currentQuestionTextHeight;
        public int CurrentQuestionTextHeight
        {
            get
            {
                return currentQuestionTextHeight;
            }
            set
            {
                if (currentQuestionTextHeight != value)
                {
                    currentQuestionTextHeight = value;
                    OnPropertyChanged("CurrentQuestionTextHeight");
                }
            }
        }


        private ObservableCollection<WhiteCard> _playerHand;
        public ObservableCollection<WhiteCard> PlayerHand
        {
            get
            {
                return _playerHand;
            }
            set
            {
                _playerHand = value;
                OnPropertyChanged("PlayerHand");
            }
        }

        private bool showPlayerHand;
        public bool ShowPlayerHand
        {
            get
            {
                return showPlayerHand;
            }
            set
            {
                if (showPlayerHand != value)
                {
                    showPlayerHand = value;
                    OnPropertyChanged("ShowPlayerHand");
                    OnPropertyChanged("EnablePlayerHand");
                }
            }
        }

        private bool enablePlayerHand;
        public bool EnablePlayerHand
        {
            get
            {
                return enablePlayerHand && showPlayerHand;
            }
            set
            {
                if (enablePlayerHand != value)
                {
                    enablePlayerHand = value;
                    OnPropertyChanged("EnablePlayerHand");
                }
            }
        }

        private string gameName;
        public string GameName
        {
            get
            {
                return gameName;
            }
            set
            {
                if (gameName != value)
                {
                    gameName = value;
                    OnPropertyChanged("GameName");
                }
            }
        }

        private int maxPlayers;
        public int MaxPlayers
        {
            get
            {
                return maxPlayers;
            }
            set
            {
                if (maxPlayers != value)
                {
                    maxPlayers = value;
                    OnPropertyChanged("MaxPlayers");
                }
            }
        }

        private ObservableCollection<Player> players;
        public ObservableCollection<Player> Players
        {
            get
            {
                return players;
            }
            set
            {
                if (players != value)
                {
                    players = value;
                    OnPropertyChanged("Players");
                }
            }
        }
        
        private bool isPlayerListOpen;
        public bool IsPlayerListOpen
        {
            get
            {
                return isPlayerListOpen;
            }
            set
            {
                if (isPlayerListOpen != value)
                {
                    isPlayerListOpen = value;
                    OnPropertyChanged("IsPlayerListOpen");
                }
            }
        }

        private string status;
        public string Status
        {
            get
            {
                return status;
            }
            set
            {
                if (status != value)
                {
                    status = value;
                    OnPropertyChanged("Status");
                }
            }
        }

        #endregion Properties

        #region Ready Command

        internal void Ready()
        {
            CardsAgainstHumility.ReadyForNextRound();
        }

        private bool CanReady()
        {
            return CardsAgainstHumility.ReadyForReview && !CardsAgainstHumility.IsReady;
        }

        public ICommand ReadyCommand { get { return new ParameterlessCommandRouter(Ready, CanReady); } }

        #endregion Ready Command

        #region Confirm Command

        internal void Confirm()
        {
            if (SelectedWhiteCard != null)
            {
                if (CardsAgainstHumility.IsCardCzar)
                    CardsAgainstHumility.SelectWinner(SelectedWhiteCard);
                else
                    CardsAgainstHumility.SelectCard(SelectedWhiteCard);
            }
        }

        private bool CanConfirm()
        {
            bool showButton = (SelectedWhiteCard != null);
            if (CardsAgainstHumility.IsCardCzar)
                showButton &= CardsAgainstHumility.RoundWinner == null;
            else
                showButton &= CardsAgainstHumility.SelectedCard == null;

            showButton &= !CardsAgainstHumility.GameOver;
            return showButton;
        }

        public ICommand ConfirmCommand { get { return new ParameterlessCommandRouter(Confirm, CanConfirm); } }

        #endregion Confirm Command

        public GameViewModel() : base()
        {
            PlayerHand = new ObservableCollection<WhiteCard>();
            CardsAgainstHumility.Game_Update += UpdateGame;

            IsPlayerListOpen = false;

            UpdatePlayerHand();
            UpdateConfirmedWhiteCard();
            UpdateCurrentQuestion();
            UpdateStatusText();
            UpdatePlayerList();
        }

        private void UpdateGame(object sender, GameUpdateEventArgs args)
        {
            dispatcher.BeginInvoke(() =>
            {
                UpdatePlayerHand();
                UpdateCurrentQuestion();
                UpdateConfirmedWhiteCard();
                UpdateStatusText();
                UpdatePlayerList();
                OnPropertyChanged("ReadyCommand");
                OnPropertyChanged("ConfirmCommand");
            });
        }

        private void UpdatePlayerHand()
        {
            var newHand = (CardsAgainstHumility.IsCardCzar || CardsAgainstHumility.ReadyToSelectWinner || CardsAgainstHumility.ReadyForReview) ? 
                CardsAgainstHumility.PlayedCards : CardsAgainstHumility.PlayerHand;
            if (newHand == null)
                newHand = new List<WhiteCard>();

            #region New Data

            var oldHandIds = PlayerHand.Select(c => c.Id).ToList();
            var newHandIds = newHand.Select(c => c.Id).ToList();
            var idsToRemove = oldHandIds.Where(c => !newHandIds.Contains(c)).ToList();
            var idsToAdd = newHandIds.Where(c => !oldHandIds.Contains(c)).ToList();
            var cardsToRemove = PlayerHand.Where(c => idsToRemove.Contains(c.Id)).ToList();
            
            foreach (var card in cardsToRemove)
            {
                PlayerHand.Remove(card);
            }

            foreach (var card in newHand.Where(c => idsToAdd.Contains(c.Id)))
            {
                PlayerHand.Add(card);
            }

            #endregion

            if (CardsAgainstHumility.GameStarted && !CardsAgainstHumility.GameOver)
            {
                if (CardsAgainstHumility.IsCardCzar)
                {
                    if (CardsAgainstHumility.ReadyToSelectWinner && CardsAgainstHumility.WinningCard == null)
                    {
                        ShowPlayerHand = true;
                        EnablePlayerHand = true;
                    }
                    else
                    {
                        ShowPlayerHand = false;
                    }
                }
                else
                {
                    if (!CardsAgainstHumility.ReadyToSelectWinner && !CardsAgainstHumility.ReadyForReview && CardsAgainstHumility.SelectedCard != null)
                    {
                        ShowPlayerHand = false;
                    }
                    else
                    {
                        ShowPlayerHand = true;
                        EnablePlayerHand = true;
                    }
                }
            }
            else
            {
                ShowPlayerHand = true;
                EnablePlayerHand = false;
            }
        }

        private void UpdateConfirmedWhiteCard()
        {
            if (CardsAgainstHumility.SelectedCard != null)
                ConfirmedWhiteCard = new WhiteCard(CardsAgainstHumility.SelectedCard, 20);
            else if (CardsAgainstHumility.IsCardCzar && CardsAgainstHumility.ReadyForReview)
                ConfirmedWhiteCard = new WhiteCard(CardsAgainstHumility.WinningCard, 20);
            else
                ConfirmedWhiteCard = null;
        }

        private void UpdateCurrentQuestion()
        {
            CurrentQuestion = (CardsAgainstHumility.GameOver ? null : CardsAgainstHumility.CurrentQuestion);
        }

        private void UpdateStatusText()
        {
            if (CardsAgainstHumility.GameStarted)
            {
                if (CardsAgainstHumility.GameOver)
                {
                    Status = $"Game Over{Environment.NewLine}{CardsAgainstHumility.Winner} Won{Environment.NewLine}Ready up to play again";
                }
                else if (CardsAgainstHumility.ReadyForReview)
                {
                    string readyStatus = $"Waiting on other players ({CardsAgainstHumility.Players.Count(c => c.IsReady)} of {CardsAgainstHumility.Players.Count})";
                    string czarNotReadyStatus = $"{CardsAgainstHumility.RoundWinner} won the round";
                    string playerNotReadyStatus = "Get Ready for the Next Round";
                    Status = CardsAgainstHumility.IsReady ? readyStatus : (CardsAgainstHumility.IsCardCzar ? czarNotReadyStatus : playerNotReadyStatus);
                }
                else if (CardsAgainstHumility.IsCardCzar)
                    Status = "You are the Card Czar";
                else if (!CardsAgainstHumility.ReadyToSelectWinner)
                    Status = (CardsAgainstHumility.SelectedCard == null ? "Select an Answer" : $"Waiting for other players ({CardsAgainstHumility.PlayersNotYetSubmitted.Count} of {CardsAgainstHumility.Players.Count})");
                else
                    Status = "Card Czar is Choosing a Winner";
            }
            else
            {
                var numPlayersNeeded = CardsAgainstHumility.RequiredNumberOfPlayers - CardsAgainstHumility.Players.Count;
                Status = $"Waiting for {numPlayersNeeded} more player{(numPlayersNeeded == 1 ? string.Empty : "s")}";
            }
        }

        private void UpdatePlayerList()
        {
            GameName = CardsAgainstHumility.GameName;
            MaxPlayers = CardsAgainstHumility.MaxPlayers;

            if (Players == null)
                Players = new ObservableCollection<Player>();
            else
                Players.Clear();
            foreach (var p in CardsAgainstHumility.Players) { Players.Add(p); }
        }
    }
}
