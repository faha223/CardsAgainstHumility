using Android.App;
using Android.OS;
using Android.Views;
using Android.Views.Animations;
using CardsAgainstHumility.GameClasses;
using CardsAgainstHumility.SocketComm;
using Newtonsoft.Json;
using Org.Json;
using SocketIO.Client;
using System;
using System.Linq;
using System.Collections.Generic;
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
        View _selectedCardView;

        WhiteCardArrayAdapter _playerHandArrayAdapter;

        Typeface tf;

        private View SelectedCard
        {
            get
            {
                return _selectedCardView;
            }
            set
            {
                if (_selectedCardView != value)
                {
                    if (_selectedCardView != null)
                        _selectedCardView.StartAnimation(deselectedCardAnimation);
                    _selectedCardView = value;
                    _selectedCardView.StartAnimation(selectedCardAnimation);
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
            
            UpdateCurrentQuestion(null);

            UpdatePlayerHand();

            CardsAgainstHumility.Game_SocketConnected += OnSocketConnected;
            CardsAgainstHumility.Game_SocketConnectError += OnSocketConnectError;
            CardsAgainstHumility.Game_SocketConnectTimeout += OnSocketConnectTimeout;
            CardsAgainstHumility.Game_Update += OnUpdateGame;
            CardsAgainstHumility.Game_Error += OnGameError;
        }

        private void UpdatePlayerHand()
        {
            RunOnUiThread(() =>
            {
                if (_playerHandArrayAdapter == null)
                {
                    _playerHandArrayAdapter = new WhiteCardArrayAdapter(this, SelectCard, CardsAgainstHumility.PlayerHand);
                    var lvPlayerHand = FindViewById<HorizontalListView>(Resource.Id.gv_PlayerHand);
                    lvPlayerHand.Adapter = _playerHandArrayAdapter;
                }
                else
                {
                    _playerHandArrayAdapter.NewData(CardsAgainstHumility.PlayerHand);
                }
            });
        }

        private void UpdateCurrentQuestion(BlackCard currentQuestion)
        {
            RunOnUiThread(() =>
            {
                if (currentQuestion == null)
                CurrentQuestionView.Visibility = ViewStates.Invisible;
            else
            {
                var text = CurrentQuestionView.FindViewById<TextView>(Resource.Id.bc_CardText);
                if (tf != null)
                    text.SetTypeface(tf, TypefaceStyle.Normal);

                text.Text = WebUtility.HtmlDecode(currentQuestion.Text);
                text.SetTextSize(Android.Util.ComplexUnitType.Dip, currentQuestion.FontSize);
                CurrentQuestionView.Visibility = ViewStates.Visible;
            }
            });
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
            UpdatePlayerHand();
            UpdateCurrentQuestion(CardsAgainstHumility.CurrentQuestion);
        }

        private void OnGameError(object sender, EventArgs args)
        {
            Console.WriteLine("Game Error: {0}", args.ToString());
        }

        private void SelectCard(WhiteCard card, View view)
        {
            Console.WriteLine("\"{0}\" selected", card.Text);
            CardsAgainstHumility.SelectCard(card);
            SelectedCard = view;
        }
    }
}