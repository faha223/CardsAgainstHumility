using Android.App;
using Android.OS;
using Android.Views;
using Android.Views.Animations;
using CardsAgainstHumility.GameClasses;
using System;
using System.Linq;
using CardsAgainstHumility.Events;
using Android.Widget;
using Android.Graphics;
using System.Net;
using Android.Util;
using Android.Support.V4.Widget;
using Android.Graphics.Drawables;

namespace CardsAgainstHumility
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

            UpdateCurrentQuestion();
            UpdateRoundWinner();
            UpdateStatusText();
            UpdateReadyButton();
            UpdatePlayerList();
            UpdateConfirmButton();
            UpdatePlayerHand();

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
            var playerHand = ((CardsAgainstHumility.IsCardCzar && CardsAgainstHumility.GameStarted) ? CardsAgainstHumility.PlayedCards : CardsAgainstHumility.PlayerHand);
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
                _playerHandArrayAdapter.NewData(playerHand);
                if (_selectedCard != null && !playerHand.Select(c => c.Id).Contains(_selectedCard.Id))
                {
                    _selectedCard = null;
                    SelectedCardView = null;
                }
                if ((!CardsAgainstHumility.IsCardCzar) && (CardsAgainstHumility.SelectedCard != null))
                    PlayerHandView.Enabled = false;
            }

            if((!CardsAgainstHumility.IsCardCzar && (CardsAgainstHumility.SelectedCard == null)) ||
                (CardsAgainstHumility.IsCardCzar && (string.IsNullOrWhiteSpace(CardsAgainstHumility.RoundWinner))))
            {
                PlayerHandView.Visibility = ViewStates.Visible;
                SelectedAnswerView.Visibility = ViewStates.Invisible;
            }
            else
            {
                WhiteCard whiteCard = _selectedCard;

                // If the player is card czar this round, and they selected a winner, display that one
                if (CardsAgainstHumility.IsCardCzar && CardsAgainstHumility.ReadyForReview && (_selectedCard == null))
                    whiteCard = new WhiteCard(CardsAgainstHumility.WinningCard, 20);
                
                PrepareWhiteCard(SelectedAnswerView, whiteCard);

                // Place the white card just below the bottom of the black card's text
                PlaceWhiteCardBelowBlackCardText(SelectedAnswerView, CurrentQuestionView);

                PlayerHandView.Visibility = ViewStates.Invisible;
                SelectedAnswerView.Visibility = ViewStates.Visible;
            }
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
                else
                    Status = null;
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
                    PrepareBlackCard(CurrentQuestionView, currentQuestion);
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
                ShowWinnerModal();
                WinnerAlertShown = true;
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
                UpdateRoundWinner();
                UpdateStatusText();
                UpdateReadyButton();
                UpdatePlayerList();
                UpdateConfirmButton();
                UpdatePlayerHand();
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
                PrepareBlackCard(blackCard, currentQuestion);

                // Prepare the white card
                var whiteCard = view.FindViewById<FrameLayout>(Resource.Id.wm_WhiteCard);
                PrepareWhiteCard(whiteCard, winningAnswer);

                // Place the white card just below the bottom of the black card's text
                PlaceWhiteCardBelowBlackCardText(whiteCard, blackCard);

                // Show the modal
                dlg.AddContentView(view, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));
                dlg.Window.SetBackgroundDrawable(new ColorDrawable(Color.Transparent));
                dlg.Show();
            });
        }

        private void PrepareBlackCard(View view, BlackCard currentQuestion)
        {
            var text = view.FindViewById<TextView>(Resource.Id.bc_CardText);
            if (UIAssets.AppFont != null)
            {
                var txtlogo = view.FindViewById<TextView>(Resource.Id.bc_logo_text);
                txtlogo.SetTypeface(UIAssets.AppFont, TypefaceStyle.Normal);
                text.SetTypeface(UIAssets.AppFont, TypefaceStyle.Normal);
            }

            if (currentQuestion != null)
            {
                text.Text = WebUtility.HtmlDecode(currentQuestion.Text);
                text.SetTextSize(Android.Util.ComplexUnitType.Dip, currentQuestion.FontSize);
            }
        }

        private void PrepareWhiteCard(View view, WhiteCard card)
        {
            var text = view.FindViewById<TextView>(Resource.Id.wc_CardText);
            if (UIAssets.AppFont != null)
            {
                var txtlogo = view.FindViewById<TextView>(Resource.Id.wc_logo_text);
                txtlogo.SetTypeface(UIAssets.AppFont, TypefaceStyle.Normal);
                text.SetTypeface(UIAssets.AppFont, TypefaceStyle.Normal);
            }

            if (card != null)
            {
                text.Text = WebUtility.HtmlDecode(card.Text);
                text.SetTextSize(Android.Util.ComplexUnitType.Dip, card.FontSize);
            }
        }

        private void PlaceWhiteCardBelowBlackCardText(View whiteCard, View blackCard)
        {
            if((blackCard != null) && (whiteCard != null))
            {
                var blackCardTextView = blackCard.FindViewById<TextView>(Resource.Id.bc_CardText);
                blackCardTextView.Measure(
                    View.MeasureSpec.MakeMeasureSpec((int)Math.Round(200 * Resources.DisplayMetrics.Density), MeasureSpecMode.AtMost),
                    View.MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified));
                whiteCard.TranslationY = blackCardTextView.MeasuredHeight + (40 * Resources.DisplayMetrics.Density);
            }
        }

        #endregion Helper Methods
    }
}