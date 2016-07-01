using Android.App;
using Android.OS;
using Android.Widget;
using Android.Util;
using Android.Graphics;

using System;
using System.Threading.Tasks;
using CardsAgainstHumility.Android.UIHelpers;
using System.Collections.Generic;
using CardsAgainstHumility.Android.Controls;
using System.Linq;
using CardsAgainstHumility.Android.ArrayAdapters;
using System.Threading;
using CardsAgainstHumility.Android.Settings;

namespace CardsAgainstHumility.Android
{
    [Activity(Label = "CreateGameActivity", Theme = "@android:style/Theme.NoTitleBar")]
    public class CreateGameActivity : Activity
    {
        Button startBtn;
        EditText gameNameTxt;
        EditText maxPlayersTxt;
        EditText pointsToWinTxt;
        ListView decksList;
        TextView decksListStatus;
        List<SelectableItem> decks;
        SettingsLoader settings;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.CreateGameMenu);
            settings = new SettingsLoader(this);

            startBtn = FindViewById<Button>(Resource.Id.cg_btnStart);
            gameNameTxt = FindViewById<EditText>(Resource.Id.cg_txtGameName);
            maxPlayersTxt = FindViewById<EditText>(Resource.Id.cg_txtMaxPlayers);
            pointsToWinTxt = FindViewById<EditText>(Resource.Id.cg_txtPointsToWin);
            decksList = FindViewById<ListView>(Resource.Id.cg_deckList);
            decksListStatus = FindViewById<TextView>(Resource.Id.cg_deckListStatus);

            var gameNameLbl = FindViewById<TextView>(Resource.Id.cg_lblGameName);
            var maxPlayersLbl = FindViewById<TextView>(Resource.Id.cg_lblMaxPlayers);
            var pointsToWinLbl = FindViewById<TextView>(Resource.Id.cg_lblPointsToWin);

            if (UIAssets.AppFont != null)
            {
                startBtn.SetTypeface(UIAssets.AppFont, TypefaceStyle.Normal);
                gameNameTxt.SetTypeface(UIAssets.AppFont, TypefaceStyle.Normal);
                maxPlayersTxt.SetTypeface(UIAssets.AppFont, TypefaceStyle.Normal);
                pointsToWinTxt.SetTypeface(UIAssets.AppFont, TypefaceStyle.Normal);
                gameNameLbl.SetTypeface(UIAssets.AppFont, TypefaceStyle.Normal);
                maxPlayersLbl.SetTypeface(UIAssets.AppFont, TypefaceStyle.Normal);
                pointsToWinLbl.SetTypeface(UIAssets.AppFont, TypefaceStyle.Normal);
                decksListStatus.SetTypeface(UIAssets.AppFont, TypefaceStyle.Normal);
            }

            gameNameTxt.Text = $"{CardsAgainstHumility.PlayerName}'s Game";
            maxPlayersTxt.Text = "10";
            pointsToWinTxt.Text = "10";

            startBtn.Enabled = false;
            startBtn.Click += async delegate
            {
                try
                {
                    await CreateGame();
                }
                catch (Exception ex)
                {
                    ShowAlert("Unexpected Error", ex.Message);
                }
            };

            var thd = new Thread(async () =>
            {
                string error = null;
                List<string> deckTitles = null;
                try
                {
                    deckTitles = await CardsAgainstHumility.GetDecks();
                }
                catch(Exception ex)
                {
                    Console.WriteLine("Error getting deck titles: {0}", ex.Message);
                    error = "Connection Error";
                }
                RunOnUiThread(() =>
                {
                    if (error == null)
                    {
                        var preferredDecks = settings.GetPreferredDecks(deckTitles);
                        decks = deckTitles.Select(c => new SelectableItem()
                        {
                            IsSelected = preferredDecks.Contains(c),
                            Text = c
                        }).ToList();
                        decksListStatus.Text = $"{deckTitles.Count} Decks Available";
                        decksList.Adapter = new SelectionListArrayAdapter(this, decks);
                        startBtn.Enabled = true;
                    }
                    else
                    {
                        decksListStatus.Text = error;
                    }
                });
            });
            thd.Start();
        }

        private async Task CreateGame()
        {
            startBtn.Enabled = false;

            try
            {
                int maxPlayers = 10;
                int pointsToWin = 5;
                if(!int.TryParse(maxPlayersTxt.Text, out maxPlayers))
                {
                    ShowAlert("Max Players", "Max Players must be an integer");
                    return;
                }
                if(!int.TryParse(pointsToWinTxt.Text, out pointsToWin))
                {
                    ShowAlert("Points to Win", "Points to win must be an integer");
                    return;
                }
                if ((maxPlayers >= 3) && (pointsToWin >= 5))
                {
                    var selectedDecks = decks.Where(c => c.IsSelected).Select(c => c.Text).ToList();
                    settings.SavePreferredDecks(selectedDecks);
                    var gid = await CardsAgainstHumility.Add(CardsAgainstHumility.NewId(), gameNameTxt.Text, selectedDecks, maxPlayers, pointsToWin);
                    CardsAgainstHumility.JoinGame(gid).Wait();
                    StartActivity(typeof(GameActivity));
                    Finish();
                }
                else
                {
                    if(maxPlayers < 3)
                        ShowAlert("Max Players", "Max Players must be at least 3");
                    else
                        ShowAlert("Points to Win", "Points to Win must be at least 5");
                }
            }
            catch (Exception ex)
            {
                Log.WriteLine(LogPriority.Error, "Exception when Creating a game", 
                    $"Message: {ex.Message}{System.Environment.NewLine}StackTrace: {ex.StackTrace}");
                throw;
            }
            finally
            {
                startBtn.Enabled = true;
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
    }
}