using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Widget;

using CardsAgainstHumility.GameClasses;
using CardsAgainstHumility.Events;
using CardsAgainstHumility.Android.ArrayAdapters;
using CardsAgainstHumility.Android.UIHelpers;

using System;
using System.Threading;

namespace CardsAgainstHumility.Android
{
    [Activity(Label = "MatchBrowserActivity", Theme = "@android:style/Theme.NoTitleBar")]
    public class GameBrowserActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.GameBrowser);
            ThreadPool.QueueUserWorkItem(o => RefreshGamesList());
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            CardsAgainstHumility.Lobby_SocketConnected -= OnSocketConnected;
            CardsAgainstHumility.Lobby_SocketConnectError -= OnSocketConnectError;
            CardsAgainstHumility.Lobby_SocketConnectTimeout -= OnSocketConnectTimeout;
            CardsAgainstHumility.Lobby_Join -= OnLobbyJoin;
            CardsAgainstHumility.Lobby_GameAdded -= OnGameAdded;
        }
        private void RefreshGamesList()
        {
            RunOnUiThread(() =>
            {
                TextView txtStatus = FindViewById<TextView>(Resource.Id.gb_Status);
                if(txtStatus != null) txtStatus.Text = "Refreshing";
                if (txtStatus != null) txtStatus.SetTypeface(UIAssets.AppFont, TypefaceStyle.Normal);
                ListView gameList = FindViewById<ListView>(Resource.Id.gb_List);
                if(gameList != null) gameList.Adapter = new GameInstanceArrayAdapter(this, null, new GameInstance[] { });
            });
            try
            {
                var games = CardsAgainstHumility.ListAsync().Result;

                RunOnUiThread(() =>
                {
                    ListView gameList = FindViewById<ListView>(Resource.Id.gb_List);
                    TextView txtStatus = FindViewById<TextView>(Resource.Id.gb_Status);

                    if(gameList != null) gameList.Adapter = new GameInstanceArrayAdapter(this, JoinGame, games);
                    if (txtStatus != null) txtStatus.SetTypeface(UIAssets.AppFont, TypefaceStyle.Normal);
                    if (txtStatus != null) txtStatus.Text = $"{((games.Count > 0) ? games.Count.ToString() : "No")} Game{(games.Count == 1 ? "" : "s")} Found";

                    CardsAgainstHumility.Lobby_SocketConnected += OnSocketConnected;
                    CardsAgainstHumility.Lobby_SocketConnectError += OnSocketConnectError;
                    CardsAgainstHumility.Lobby_SocketConnectTimeout += OnSocketConnectTimeout;
                    CardsAgainstHumility.Lobby_Join += OnLobbyJoin;
                    CardsAgainstHumility.Lobby_GameAdded += OnGameAdded;
                    CardsAgainstHumility.ConnectToLobby();
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                RunOnUiThread(() =>
                {
                    TextView txtStatus = FindViewById<TextView>(Resource.Id.gb_Status);
                    if(txtStatus != null) txtStatus.Text = ("Connection Error");
                });
            }
        }

        private void JoinGame(GameInstance gi)
        {
            try
            {
                CardsAgainstHumility.JoinGame(gi.Id).Wait();
                StartActivity(typeof(GameActivity));
                Finish();
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred when trying to join {0}: {1}", gi.Name, ex.Message);
            }
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

        private void OnLobbyJoin(object sender, EventArgs args)
        {
            Console.WriteLine("Lobby Joined");
        }

        private void OnGameAdded(object sender, GameAddedEventArgs args)
        {
            RunOnUiThread(() =>
            {
                ListView gameList = FindViewById<ListView>(Resource.Id.gb_List);
                TextView txtStatus = FindViewById<TextView>(Resource.Id.gb_Status);
                if(gameList != null) gameList.Adapter = new GameInstanceArrayAdapter(this, JoinGame, args.Games);
                if(txtStatus != null) txtStatus.Text = $"{((args.Games.Count > 0) ? args.Games.Count.ToString() : "No")} Game{(args.Games.Count == 1 ? "" : "s")} Found";
            });
        }
    }
}