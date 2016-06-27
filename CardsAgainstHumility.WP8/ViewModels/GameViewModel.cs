﻿using CardsAgainstHumility.GameClasses;
using CardsAgainstHumility.Helpers;
using CardsAgainstHumility.WP8.MVVM_Helpers;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Linq;
using CardsAgainstHumility.Events;
using System.Collections.Generic;
using System;

namespace CardsAgainstHumility.WP8.ViewModels
{
    class GameViewModel : ViewModelBase
    {
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
                if (confirmedWhiteCard != value)
                {
                    confirmedWhiteCard = value;
                    OnPropertyChanged("ConfirmedWhiteCard");
                }
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


        #region Ready Command

        internal void Ready()
        {

        }

        private bool CanReady()
        {
            return CardsAgainstHumility.ReadyForReview && !CardsAgainstHumility.IsReady;
        }

        public ICommand ReadyCommand { get { return new ParameterlessCommandRouter(Ready, CanReady); } }

        #endregion Ready Command

        #region ConfirmCommand

        internal void Confirm()
        {
            CardsAgainstHumility.SelectCard(SelectedWhiteCard);
        }

        private bool CanConfirm()
        {
            return (SelectedWhiteCard != null) && (ConfirmedWhiteCard == null);
        }

        public ICommand ConfirmCommand { get { return new ParameterlessCommandRouter(Confirm, CanConfirm); } }

        #endregion ConfirmCommand

        public GameViewModel() : base()
        {
            PlayerHand = new ObservableCollection<WhiteCard>();
            CardsAgainstHumility.Game_Update += UpdateGame;

            UpdatePlayerHand();
            UpdateConfirmedWhiteCard();
            UpdateCurrentQuestion();
            UpdateStatusText();
        }

        private void UpdateGame(object sender, GameUpdateEventArgs args)
        {
            dispatcher.BeginInvoke(() =>
            {
                UpdatePlayerHand();
                UpdateConfirmedWhiteCard();
                UpdateCurrentQuestion();
                UpdateStatusText();
            });
        }

        private void UpdatePlayerHand()
        {
            var newHand = CardsAgainstHumility.PlayerHand;
            if (newHand == null)
                newHand = new List<WhiteCard>();

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
        }

        private void UpdateConfirmedWhiteCard()
        {
            if (CardsAgainstHumility.SelectedCard != null)
                ConfirmedWhiteCard = new WhiteCard(CardsAgainstHumility.SelectedCard, 20);
        }

        private void UpdateCurrentQuestion()
        {
            CurrentQuestion = CardsAgainstHumility.CurrentQuestion;
        }

        private void UpdateStatusText()
        {
            if(CardsAgainstHumility.GameOver)
            {
                Status = $"Game Over{Environment.NewLine}{CardsAgainstHumility.Winner} Won{Environment.NewLine}Ready up to Start the next Game";
            }
            if(CardsAgainstHumility.GameStarted)
            {
                if(CardsAgainstHumility.IsCardCzar)
                {
                    if (CardsAgainstHumility.ReadyToSelectWinner)
                        Status = "Select a Winner";
                    else
                        Status = "You are Card Czar";
                }
                else
                {
                    if (CardsAgainstHumility.SelectedCard == null)
                        Status = "Select an Answer";
                    else
                        Status = $"Waiting for other players ({CardsAgainstHumility.Players.Count - CardsAgainstHumility.PlayedCards.Count} of {CardsAgainstHumility.Players.Count})";
                }
            }
            else
            {
                Status = $"Waiting for {CardsAgainstHumility.RequiredNumberOfPlayers - CardsAgainstHumility.Players.Count} more players";
            }
        }
    }
}
