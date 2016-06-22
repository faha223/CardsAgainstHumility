using Android.App;
using Android.OS;
using Android.Widget;
using Android.Util;
using Android.Graphics;

using System;
using System.Threading.Tasks;
using CardsAgainstHumility.Android.UIHelpers;

namespace CardsAgainstHumility.Android
{
    [Activity(Label = "CreateGameActivity", Theme = "@android:style/Theme.NoTitleBar")]
    public class CreateGameActivity : Activity
    {
        Button startBtn;
        EditText gameNameTxt;
        EditText maxPlayersTxt;
        EditText pointsToWinTxt;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.CreateGameMenu);

            startBtn = FindViewById<Button>(Resource.Id.cg_btnStart);
            gameNameTxt = FindViewById<EditText>(Resource.Id.cg_txtGameName);
            maxPlayersTxt = FindViewById<EditText>(Resource.Id.cg_txtMaxPlayers);
            pointsToWinTxt = FindViewById<EditText>(Resource.Id.cg_txtPointsToWin);
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
            }

            gameNameTxt.Text = $"{CardsAgainstHumility.PlayerName}'s Game";
            maxPlayersTxt.Text = "10";
            pointsToWinTxt.Text = "10";

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
                    var gid = await CardsAgainstHumility.Add(CardsAgainstHumility.NewId(), gameNameTxt.Text, maxPlayers, pointsToWin);
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