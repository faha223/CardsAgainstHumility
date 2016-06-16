using Android.App;
using Android.OS;
using Android.Views;
using Android.Views.Animations;
using CardsAgainstHumility.GameClasses;
using System;
using CardsAgainstHumility.Events;
using Android.Widget;
using Android.Graphics;
using System.Net;
using Android.Util;

namespace CardsAgainstHumility
{
    [Activity(Label = "GameActivity", Theme = "@android:style/Theme.NoTitleBar")]
    public class GameActivity : Activity
    {
        HorizontalListView PlayerHandView;
        TextView cardCzarIndicator;
        Button readyButton;

        WhiteCardArrayAdapter _playerHandArrayAdapter;
        WhiteCardArrayAdapter _playedCardsArrayAdapter;

        Typeface tf;

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
            
            try
            {
                tf = Typeface.CreateFromAsset(Assets, "Helvetica-Bold.ttf");
            }
            catch { }

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

            UpdateCurrentQuestion();
            UpdatePlayerHand();
            UpdateReadyButton();

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
            }
            if (cardCzarIndicator == null)
            {
                cardCzarIndicator = FindViewById<TextView>(Resource.Id.gv_CardCzar);
                if (tf != null)
                    cardCzarIndicator.SetTypeface(tf, TypefaceStyle.Normal);
            }
            cardCzarIndicator.Visibility = (CardsAgainstHumility.IsCardCzar ? ViewStates.Visible : ViewStates.Invisible);
        }

        private void UpdateCurrentQuestion()
        {
            var currentQuestion = CardsAgainstHumility.CurrentQuestion;
            if (currentQuestion == null)
                CurrentQuestionView.Visibility = ViewStates.Invisible;
            else
            {
                var text = CurrentQuestionView.FindViewById<TextView>(Resource.Id.bc_CardText);
                if (tf != null)
                {
                    var txtlogo = CurrentQuestionView.FindViewById<TextView>(Resource.Id.bc_logo_text);
                    txtlogo.SetTypeface(tf, TypefaceStyle.Normal);
                    text.SetTypeface(tf, TypefaceStyle.Normal);
                }

                text.Text = WebUtility.HtmlDecode(currentQuestion.Text);
                text.SetTextSize(Android.Util.ComplexUnitType.Dip, currentQuestion.FontSize);
                CurrentQuestionView.Visibility = ViewStates.Visible;
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
                };
            }
            readyButton.Visibility = (CardsAgainstHumility.ReadyForReview) ? ViewStates.Visible : ViewStates.Invisible;
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
                UpdateReadyButton();
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
                if (CardsAgainstHumility.IsCardCzar)
                {
                    if (CardsAgainstHumility.ReadyToSelectWinner)
                        CardsAgainstHumility.SelectWinner(card);
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
                        Console.WriteLine("User played \"{0}\" ", card.Id);
                        CardsAgainstHumility.SelectCard(card);
                    }
                }
            }
        }
    }
}