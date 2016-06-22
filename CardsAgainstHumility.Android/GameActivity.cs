using Android.App;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Text;
using Android.Util;
using Android.Views.Animations;
using Android.Views;
using Android.Widget;

using CardsAgainstHumility.Android.ArrayAdapters;
using CardsAgainstHumility.Android.Controls;
using CardsAgainstHumility.Events;
using CardsAgainstHumility.GameClasses;

using System;
using System.Collections.Generic;
using System.Linq;
using CardsAgainstHumility.Android.UIHelpers;

namespace CardsAgainstHumility.Android
{
    [Activity(Label = "GameActivity", Theme = "@android:style/Theme.NoTitleBar")]
    public class GameActivity : Activity
    {
        #region Members

        HorizontalListView PlayerHandView;
        TextView statusIndicator;
        Button readyButton;
        Button confirmButton;
        WhiteCardArrayAdapter _playerHandArrayAdapter;
        ListView PlayerList;
        TextView PlayerListHeader;
        PlayerArrayAdapter PlayerArrayAdapter;
        DrawerLayout _drawer;
        bool WinnerAlertShown = false;
        View CurrentQuestionView;
        View SelectedAnswerView;

        #endregion Members

        #region Properties

        WhiteCard _selectedCard;
        View _selectedCardView;
        View SelectedCardView
        {
            get
            {
                return _selectedCardView;
            }
            set
            {
                if (_selectedCardView != value)
                {
                    if(_selectedCardView != null)
                        _selectedCardView.StartAnimation(deselectedCardAnimation);

                    _selectedCardView = value;

                    if(_selectedCardView != null)
                        _selectedCardView.StartAnimation(selectedCardAnimation);

                    RunOnUiThread(() =>
                    {
                        UpdateConfirmButton();
                    });
                }
            }
        }

        private string Status
        {
            get
            {
                if(statusIndicator != null)
                    return statusIndicator.Text;
                return null;
            }
            set
            {
                if (statusIndicator == null)
                    return;
                if (statusIndicator.Text != value)
                {
                    statusIndicator.Text = value;
                    if (string.IsNullOrWhiteSpace(statusIndicator.Text))
                        statusIndicator.Visibility = ViewStates.Invisible;
                    else
                        statusIndicator.Visibility = ViewStates.Visible;
                }
            }
        }

        #endregion Properties

        #region Animations

        private Animation selectedCardAnimation
        {
            get
            {
                var anim =  new TranslateAnimation(0, 0, 0, -75);
                anim.Duration = 100;
                anim.FillAfter = true;
                return anim;
            }
        }

        private Animation deselectedCardAnimation
        {
            get
            {
                var anim = new TranslateAnimation(0, 0, -75, 0);
                anim.Duration = 100;
                anim.FillAfter = true;
                return anim;
            }
        }

        #endregion Animations

        #region Activity

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.GameView);

            CurrentQuestionView = FindViewById(Resource.Id.gv_CurrentQuestion);
            SelectedAnswerView = FindViewById(Resource.Id.gv_SelectedCard);

            PlayerListHeader = FindViewById<TextView>(Resource.Id.gv_PlayerCount);
            if (UIAssets.AppFont != null)
                PlayerListHeader.SetTypeface(UIAssets.AppFont, TypefaceStyle.Normal);

            PlayerList = FindViewById<ListView>(Resource.Id.gv_playerList);
            PlayerArrayAdapter = new PlayerArrayAdapter(this, CardsAgainstHumility.Players);
            PlayerList.Adapter = PlayerArrayAdapter;

            _drawer = FindViewById<DrawerLayout>(Resource.Id.gv_drawer);
            _drawer.DrawerOpened += (sender, args) =>
            {
                _drawer.Invalidate();
            };

            FindViewById<TextView>(Resource.Id.gv_GameName).Text = CardsAgainstHumility.GameName;
            if (UIAssets.AppFont != null)
                FindViewById<TextView>(Resource.Id.gv_GameName).SetTypeface(UIAssets.AppFont, TypefaceStyle.Normal);

            FindViewById(Resource.Id.gv_playerListBtn).Click += (sender, args) =>
            {
                _drawer.OpenDrawer((int)GravityFlags.Start);
            };

            UpdateCurrentQuestion();
            UpdateStatusText();
            UpdateReadyButton();
            UpdatePlayerList();
            UpdateConfirmButton();
            UpdatePlayerHand();
            UpdateSelectedCardView();
            UpdateRoundWinner();

            CardsAgainstHumility.Game_SocketConnected += OnSocketConnected;
            CardsAgainstHumility.Game_SocketConnectError += OnSocketConnectError;
            CardsAgainstHumility.Game_SocketConnectTimeout += OnSocketConnectTimeout;
            CardsAgainstHumility.Game_Update += OnUpdateGame;
            CardsAgainstHumility.Game_Error += OnGameError;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            // If isFinishing is false, the player isn't really leaving the game
            if(IsFinishing)
                CardsAgainstHumility.DepartGame();

            CardsAgainstHumility.Game_SocketConnected -= OnSocketConnected;
            CardsAgainstHumility.Game_SocketConnectError -= OnSocketConnectError;
            CardsAgainstHumility.Game_SocketConnectTimeout -= OnSocketConnectTimeout;
            CardsAgainstHumility.Game_Update -= OnUpdateGame;
            CardsAgainstHumility.Game_Error -= OnGameError;

            CurrentQuestionView.Dispose();
            CurrentQuestionView = null;
        }

        #endregion Activity

        #region Update Game Logic

        private void UpdatePlayerHand()
        {
            List<WhiteCard> playerHand;

            // Get the correct set of cards to display to the user
            if (CardsAgainstHumility.GameStarted)
            {
                if (CardsAgainstHumility.IsCardCzar)
                    playerHand = (CardsAgainstHumility.ReadyToSelectWinner ? CardsAgainstHumility.PlayedCards : new List<WhiteCard>());
                else
                    playerHand = (CardsAgainstHumility.ReadyToSelectWinner ? CardsAgainstHumility.PlayedCards : CardsAgainstHumility.PlayerHand);
            }
            else
                playerHand = CardsAgainstHumility.PlayerHand;
            
            if (_playerHandArrayAdapter == null)
            {
                _playerHandArrayAdapter = new WhiteCardArrayAdapter(this, SelectCard, playerHand);
                PlayerHandView = FindViewById<HorizontalListView>(Resource.Id.gv_PlayerHand);
                PlayerHandView.Adapter = _playerHandArrayAdapter;
                PlayerHandView.ItemSelected += (sender, args) =>
                {
                    if ((args.Position >= 0) && (args.Position < CardsAgainstHumility.PlayerHand.Count))
                    {
                        SelectCard(playerHand[args.Position], args.View);
                    }
                };
            }
            else
            {
                bool completelyReplaced = _playerHandArrayAdapter.NewData(playerHand);
                if (completelyReplaced)
                    PlayerHandView.ScrollTo(0);

                if (_selectedCard != null && !playerHand.Select(c => c.Id).Contains(_selectedCard.Id))
                {
                    _selectedCard = null;
                    SelectedCardView = null;
                }
                if ((!CardsAgainstHumility.IsCardCzar) && (CardsAgainstHumility.SelectedCard != null))
                    PlayerHandView.Enabled = false;
            }

            if(CardsAgainstHumility.GameStarted)
            {
                if(CardsAgainstHumility.IsCardCzar)
                {
                    // If the player is card czar, only show the "hand" while they are supposed to be choosing a winner
                    if (CardsAgainstHumility.ReadyToSelectWinner && string.IsNullOrWhiteSpace(CardsAgainstHumility.WinningCard))
                        PlayerHandView.Visibility = ViewStates.Visible;
                    else
                        PlayerHandView.Visibility = ViewStates.Invisible;
                }
                else
                {
                    // If the player has selected their card and is waiting on the other players, don't show the hand, otherwise, show the hand
                    if (!CardsAgainstHumility.ReadyToSelectWinner && !CardsAgainstHumility.ReadyForReview && CardsAgainstHumility.SelectedCard != null)
                        PlayerHandView.Visibility = ViewStates.Invisible;
                    else
                        PlayerHandView.Visibility = ViewStates.Visible;
                }
            }
            else // Let the player view their hand if the game hasn't started yet
                PlayerHandView.Visibility = ViewStates.Visible;
        }

        private void UpdateSelectedCardView()
        {
            bool showSelectedCardView = false;
            WhiteCard whiteCard = _selectedCard;
            if (CardsAgainstHumility.GameStarted)
            {
                if(CardsAgainstHumility.IsCardCzar)
                {
                    if(!string.IsNullOrWhiteSpace(CardsAgainstHumility.WinningCard))
                    {
                        if (whiteCard == null)
                            whiteCard = new WhiteCard(CardsAgainstHumility.WinningCard, 20);
                        showSelectedCardView = true;
                    }
                }
                else
                {
                    if(CardsAgainstHumility.SelectedCard != null)
                    {
                        showSelectedCardView = true;
                    }
                }
            }

            if (showSelectedCardView)
            {
                UIAssets.PrepareWhiteCard(SelectedAnswerView, whiteCard);
                // Place the white card just below the bottom of the black card's text
                PlaceWhiteCardBelowBlackCardText(SelectedAnswerView, CurrentQuestionView);
                SelectedAnswerView.Visibility = ViewStates.Visible;
            }
            else
                SelectedAnswerView.Visibility = ViewStates.Invisible;
        }

        private void UpdateStatusText()
        {
            if (statusIndicator == null)
            {
                statusIndicator = FindViewById<TextView>(Resource.Id.gv_CardCzar);
                if (UIAssets.AppFont != null)
                    statusIndicator.SetTypeface(UIAssets.AppFont, TypefaceStyle.Normal);
            }

            if (CardsAgainstHumility.GameStarted)
            {
                if (CardsAgainstHumility.ReadyForReview)
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

        private void UpdateCurrentQuestion()
        {
            var currentQuestion = CardsAgainstHumility.CurrentQuestion;
            if (CurrentQuestionView != null)
            {
                if (currentQuestion == null)
                    CurrentQuestionView.Visibility = ViewStates.Invisible;
                else
                {
                    UIAssets.PrepareBlackCard(CurrentQuestionView, currentQuestion);
                    CurrentQuestionView.Visibility = ViewStates.Visible;
                }
            }
        }

        private void UpdateReadyButton()
        {
            if (readyButton == null)
            {
                readyButton = FindViewById<Button>(Resource.Id.gv_ReadyButton);
                readyButton.SetTypeface(UIAssets.AppFont, TypefaceStyle.Normal);
                readyButton.Click += (sender, args) =>
                {
                    CardsAgainstHumility.ReadyForNextRound();
                    WinnerAlertShown = false;
                };
            }
            readyButton.Visibility = (CardsAgainstHumility.ReadyForReview && !CardsAgainstHumility.IsReady) ? ViewStates.Visible : ViewStates.Invisible;
        }

        private void UpdateConfirmButton()
        {
            if(confirmButton == null)
            {
                confirmButton = FindViewById<Button>(Resource.Id.gv_ConfirmButton);
                confirmButton.SetTypeface(UIAssets.AppFont, TypefaceStyle.Normal);
                confirmButton.Click += ConfirmCard;
            }
            bool showButton = (_selectedCard != null);
            if (CardsAgainstHumility.IsCardCzar)
                showButton &= string.IsNullOrWhiteSpace(CardsAgainstHumility.RoundWinner);
            else
                showButton &= CardsAgainstHumility.SelectedCard == null;

            confirmButton.Visibility = (showButton) ? ViewStates.Visible : ViewStates.Invisible;
        }

        private void UpdatePlayerList()
        {
            PlayerListHeader.Text = $"Players ({CardsAgainstHumility.Players.Count} of {CardsAgainstHumility.MaxPlayers})";
            PlayerArrayAdapter.NewData(CardsAgainstHumility.Players);
        }

        private void UpdateRoundWinner()
        {
            // Don't show this if the player is the card czar. They already know.
            // Don't show this if you already showed it this round
            // Don't show this if the player is already ready
            // Don't show this if the round hasn't ended yet

            if(!CardsAgainstHumility.IsCardCzar && CardsAgainstHumility.ReadyForReview && !CardsAgainstHumility.IsReady && !WinnerAlertShown)
            {
                WinnerAlertShown = true;
                ShowWinnerModal();
            }
        }

        #endregion Update Game Logic

        #region CardsAgainstHumility Event Handlers

        private void OnSocketConnected(object sender, EventArgs args)
        {
            Console.WriteLine("Socket Connected");
        }

        private void OnSocketConnectError(object sender, SocketConnectErrorEventArgs args)
        {
            Console.WriteLine("Socket Connect Error: {0}", args.Message);
        }

        private void OnSocketConnectTimeout(object sender, SocketConnectErrorEventArgs args)
        {
            Console.WriteLine("Socket Connect Timed Out");
        }

        private void OnUpdateGame(object sender, GameUpdateEventArgs args)
        {
            Console.WriteLine("Game Updated");
            RunOnUiThread(() =>
            {
                UpdateCurrentQuestion();
                UpdateStatusText();
                UpdateReadyButton();
                UpdatePlayerList();
                UpdateConfirmButton();
                UpdatePlayerHand();
                UpdateSelectedCardView();
                UpdateRoundWinner();
            });
        }

        private void OnGameError(object sender, EventArgs args)
        {
            Console.WriteLine("Game Error: {0}", args.ToString());
        }

        #endregion CardsAgainstHumility Event Handlers

        #region Game Events

        private void SelectCard(WhiteCard card, View view)
        {
            if (CardsAgainstHumility.GameStarted && (CardsAgainstHumility.SelectedCard == null))
            {
                _selectedCard = card;
                SelectedCardView = view;
            }
        }

        private void ConfirmCard(object sender, EventArgs args)
        {
            if (CardsAgainstHumility.IsCardCzar && _selectedCard != null)
            {
                if (CardsAgainstHumility.ReadyToSelectWinner)
                    CardsAgainstHumility.SelectWinner(_selectedCard);
                else
                    RunOnUiThread(() =>
                    {
                        var builder = new AlertDialog.Builder(this);
                        builder.SetTitle("Still waiting on");
                        builder.SetMessage(string.Join(System.Environment.NewLine, CardsAgainstHumility.PlayersNotYetSubmitted));
                        builder.Create().Show();
                    });
            }
            else
            {
                if (CardsAgainstHumility.SelectedCard == null)
                {
                    Console.WriteLine("User played \"{0}\" ", _selectedCard.Id);
                    CardsAgainstHumility.SelectCard(_selectedCard);
                }
            }
        }

        #endregion Game Events

        #region Helper Methods

        private void ShowAlert(string caption, string message)
        {
            RunOnUiThread(() =>
            {
                var builder = new AlertDialog.Builder(this);
                builder.SetTitle(caption);
                builder.SetMessage(message);
                builder.Create().Show();
            });
        }

        private void ShowWinnerModal()
        {
            var RoundWinner = (CardsAgainstHumility.RoundWinner == CardsAgainstHumility.PlayerName ? "You" : CardsAgainstHumility.RoundWinner);
            var currentQuestion = CardsAgainstHumility.CurrentQuestion;
            var winningAnswer = new WhiteCard(CardsAgainstHumility.WinningCard, 20);

            RunOnUiThread(() =>
            {
                // Prepare the builder
                var dlg = new Dialog(this);
                var view = LayoutInflater.Inflate(Resource.Layout.RoundWinnerAnnouncement, null);

                // Show the winner's name
                var winnerName = view.FindViewById<TextView>(Resource.Id.wm_Winner);
                winnerName.SetTypeface(UIAssets.AppFont, TypefaceStyle.Normal);
                winnerName.Text = $"{RoundWinner} won the round";
                winnerName.SetTextSize(ComplexUnitType.Dip, 16);

                // Prepare the black card
                var blackCard = view.FindViewById<View>(Resource.Id.wm_BlackCard);
                UIAssets.PrepareBlackCard(blackCard, currentQuestion);

                // Prepare the white card
                var whiteCard = view.FindViewById<FrameLayout>(Resource.Id.wm_WhiteCard);
                UIAssets.PrepareWhiteCard(whiteCard, winningAnswer);

                // Place the white card just below the bottom of the black card's text
                PlaceWhiteCardBelowBlackCardText(whiteCard, blackCard);

                // Link up the close button
                var closeBtn = view.FindViewById<Button>(Resource.Id.wm_closeBtn);
                closeBtn.SetTypeface(UIAssets.AppFont, TypefaceStyle.Normal);
                closeBtn.Click += (owner, args) =>
                {
                    dlg.Dismiss();
                };

                // Show the modal
                dlg.AddContentView(view, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));
                dlg.Window.SetBackgroundDrawable(new ColorDrawable(Color.Transparent));
                dlg.Show();
            });
        }

        private void PlaceWhiteCardBelowBlackCardText(View whiteCard, View blackCard)
        {
            if((blackCard != null) && (whiteCard != null))
            {
                var blackCardTextView = blackCard.FindViewById<TextView>(Resource.Id.bc_CardText);
                blackCardTextView.Measure(
                    View.MeasureSpec.MakeMeasureSpec((int)Math.Round(200 * Resources.DisplayMetrics.Density), MeasureSpecMode.AtMost),
                    View.MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified));
                whiteCard.TranslationY = blackCardTextView.MeasuredHeight + (30 * Resources.DisplayMetrics.Density);
            }
        }

        #endregion Helper Methods
    }
}