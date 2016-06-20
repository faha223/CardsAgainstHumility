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
        HorizontalListView PlayerHandView;
        TextView statusIndicator;
        Button readyButton;
        WhiteCardArrayAdapter _playerHandArrayAdapter;
        ListView PlayerList;
        TextView PlayerListHeader;
        PlayerArrayAdapter PlayerArrayAdapter;
        DrawerLayout _drawer;
        bool WinnerAlertShown = false;

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
                    {
                        _selectedCardView.Click -= ConfirmCard;
                        _selectedCardView.StartAnimation(deselectedCardAnimation);
                    }
                    _selectedCardView = value;
                    _selectedCardView.StartAnimation(selectedCardAnimation);
                    _selectedCardView.Click += ConfirmCard;
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

        private View CurrentQuestionView;

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

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.GameView);

            var _currentQuestionHolder = FindViewById<RelativeLayout>(Resource.Id.gv_CurrentQuestion);
            CurrentQuestionView = ((LayoutInflater)GetSystemService(LayoutInflaterService)).Inflate(Resource.Layout.BlackCard, null);
            CurrentQuestionView.SetZ(-10);
            var layoutParams = new RelativeLayout.LayoutParams(
                ViewGroup.LayoutParams.WrapContent, 
                ViewGroup.LayoutParams.WrapContent);
            layoutParams.AddRule(LayoutRules.CenterInParent);
            layoutParams.AddRule(LayoutRules.AlignParentTop);
            layoutParams.AddRule(LayoutRules.CenterHorizontal);
            layoutParams.SetMargins(0, (int)Math.Round(TypedValue.ApplyDimension(ComplexUnitType.Dip, 50, Resources.DisplayMetrics)), 0, 0);
            CurrentQuestionView.LayoutParameters = layoutParams;
            _currentQuestionHolder.AddView(CurrentQuestionView);

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
            UpdatePlayerHand();
            UpdateReadyButton();
            UpdateStatusText();
            UpdatePlayerList();

            CardsAgainstHumility.Game_SocketConnected += OnSocketConnected;
            CardsAgainstHumility.Game_SocketConnectError += OnSocketConnectError;
            CardsAgainstHumility.Game_SocketConnectTimeout += OnSocketConnectTimeout;
            CardsAgainstHumility.Game_Update += OnUpdateGame;
            CardsAgainstHumility.Game_Error += OnGameError;
        }

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
                if ((!CardsAgainstHumility.IsCardCzar) && (CardsAgainstHumility.SelectedCard != null))
                    PlayerHandView.Enabled = false;
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
                    Status = CardsAgainstHumility.IsReady ? $"Waiting on other players ({CardsAgainstHumility.Players.Count(c => c.IsReady)} of {CardsAgainstHumility.Players.Count})" : "Get Ready for the Next Round";
                }
                else if (CardsAgainstHumility.IsCardCzar)
                    Status = "You are the Card Czar";
                else
                    Status = null;
            }
            else
            {
                Status = $"Waiting for more players ({CardsAgainstHumility.Players.Count} of {CardsAgainstHumility.RequiredNumberOfPlayers})";
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
                readyButton.Click += (sender, args) =>
                {
                    CardsAgainstHumility.ReadyForNextRound();
                    WinnerAlertShown = false;
                };
            }
            readyButton.Visibility = (CardsAgainstHumility.ReadyForReview && !CardsAgainstHumility.IsReady) ? ViewStates.Visible : ViewStates.Invisible;
        }

        private void UpdatePlayerList()
        {
            PlayerListHeader.Text = $"Players ({CardsAgainstHumility.Players.Count} of {CardsAgainstHumility.MaxPlayers})";
            PlayerArrayAdapter.NewData(CardsAgainstHumility.Players);
        }

        private void UpdateRoundWinner()
        {
            if(CardsAgainstHumility.ReadyForReview && !CardsAgainstHumility.IsReady && !WinnerAlertShown)
            {
                ShowWinnerModal();
                WinnerAlertShown = true;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            CardsAgainstHumility.DepartGame();
            CardsAgainstHumility.Game_SocketConnected -= OnSocketConnected;
            CardsAgainstHumility.Game_SocketConnectError -= OnSocketConnectError;
            CardsAgainstHumility.Game_SocketConnectTimeout -= OnSocketConnectTimeout;
            CardsAgainstHumility.Game_Update -= OnUpdateGame;
            CardsAgainstHumility.Game_Error -= OnGameError;

            CurrentQuestionView.Dispose();
            CurrentQuestionView = null;
        }

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
                UpdatePlayerHand();
                UpdateCurrentQuestion();
                UpdateStatusText();
                UpdateReadyButton();
                UpdatePlayerList();
                UpdateRoundWinner();
            });
        }

        private void OnGameError(object sender, EventArgs args)
        {
            Console.WriteLine("Game Error: {0}", args.ToString());
        }

        private void SelectCard(WhiteCard card, View view)
        {
            if (CardsAgainstHumility.GameStarted)
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

        private void PrepareBlackCard(View view, BlackCard currentQuestion)
        {
            var text = view.FindViewById<TextView>(Resource.Id.bc_CardText);
            if (UIAssets.AppFont != null)
            {
                var txtlogo = view.FindViewById<TextView>(Resource.Id.bc_logo_text);
                txtlogo.SetTypeface(UIAssets.AppFont, TypefaceStyle.Normal);
                text.SetTypeface(UIAssets.AppFont, TypefaceStyle.Normal);
            }

            text.Text = WebUtility.HtmlDecode(currentQuestion.Text);
            text.SetTextSize(Android.Util.ComplexUnitType.Dip, currentQuestion.FontSize);
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

            text.Text = WebUtility.HtmlDecode(card.Text);
            text.SetTextSize(Android.Util.ComplexUnitType.Dip, card.FontSize);
        }

        private void ShowWinnerModal()
        {
            RunOnUiThread(() =>
            {
                // Prepare the builder
                var builder = new AlertDialog.Builder(this);
                var view = LayoutInflater.Inflate(Resource.Layout.RoundWinnerAnnouncement, null);

                // Show the winner's name
                var winnerName = view.FindViewById<TextView>(Resource.Id.wm_Winner);
                winnerName.SetTypeface(UIAssets.AppFont, TypefaceStyle.Normal);
                winnerName.Text = $"{CardsAgainstHumility.RoundWinner} won the round";

                // Prepare the black card
                var blackCardView = view.FindViewById<FrameLayout>(Resource.Id.wm_BlackCard);
                var blackCard = LayoutInflater.Inflate(Resource.Layout.BlackCard, null);
                PrepareBlackCard(blackCard, CardsAgainstHumility.CurrentQuestion);
                blackCard.ScaleX = blackCard.ScaleY = 0.65f;
                blackCardView.AddView(blackCard);

                // Prepare the white card
                var whiteCardView = view.FindViewById<FrameLayout>(Resource.Id.wm_WhiteCard);
                var whiteCard = LayoutInflater.Inflate(Resource.Layout.WhiteCard, null);
                PrepareWhiteCard(whiteCard, new WhiteCard(CardsAgainstHumility.WinningCard, 20));
                whiteCard.ScaleX = whiteCard.ScaleY = 0.65f;
                whiteCardView.AddView(whiteCard);

                // Show the modal
                builder.SetView(view);
                var dlg = builder.Create();
                dlg.Window.SetBackgroundDrawable(new ColorDrawable(Color.Transparent));
                dlg.Show();
            });
        }
    }
}