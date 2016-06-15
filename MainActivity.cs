using System;
using Android.App;
using Android.Widget;
using Android.OS;
using Android.Graphics;
using Android.Util;
using System.Threading.Tasks;

namespace CardsAgainstHumility
{
    [Activity(Label = "Cards Against Humility", MainLauncher = true, Icon = "@drawable/icon", Theme = "@android:style/Theme.NoTitleBar")]
    public class MainActivity : Activity
    {
        AlertDialog.Builder dialogBuilder { get; set; }

        Button createButton;
        Button joinButton;
        Button settingsButton;
        Button quitButton;

        protected override void OnCreate(Bundle bundle)
        {
            CardsAgainstHumility.InitDefaultValues(this);

            dialogBuilder = new AlertDialog.Builder(this, 0);

            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            Typeface tf;
            try
            {
                tf = Typeface.CreateFromAsset(Assets, "Helvetica-Bold.ttf");
            }
            catch (Exception e)
            {
                Log.Error("TextView", string.Format("Could not get Typeface: Helvetica.ttf Error: {0}", e));
                return;
            }

            var tv = FindViewById<TextView>(Resource.Id.main_logo1);
            if (tv != null)
                tv.SetTypeface(tf, TypefaceStyle.Normal);
            tv = FindViewById<TextView>(Resource.Id.main_logo2);
            if (tv != null)
                tv.SetTypeface(tf, TypefaceStyle.Normal);
            tv = FindViewById<TextView>(Resource.Id.main_logo3);
            if (tv != null)
                tv.SetTypeface(tf, TypefaceStyle.Normal);

            createButton = FindViewById<Button>(Resource.Id.main_btnCreateGame);
            joinButton = FindViewById<Button>(Resource.Id.main_btnJoinGame);
            settingsButton = FindViewById<Button>(Resource.Id.main_btnSettings);
            quitButton = FindViewById<Button>(Resource.Id.main_btnQuitGame);

            if (createButton != null)
            {
                createButton.Click += async delegate
                {
                    try
                    {
                        await CreateGame().ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        ShowAlert("An error occurred while creating a game", ex.Message);
                    }
                };
            }
            else
                Console.WriteLine("Unable to get Create Button");

            if (joinButton != null)
            {
                joinButton.Click += delegate
                {
                    StartActivity(typeof(GameBrowserActivity));
                };
            }
            else
                Console.WriteLine("Unable to get Join Button");

            if (settingsButton != null)
            {
                settingsButton.Click += delegate
                {
                    StartActivity(typeof(SettingsActivity));
                };
            }
            else
                Console.WriteLine("Unable to get Settings Button");

            if (quitButton != null)
            {
                quitButton.Click += delegate
                {
                    Finish();
                };
            }
            else
                Console.WriteLine("Unable to get Quit Button");
        }

        private async Task CreateGame()
        {
            createButton.Enabled = false;
            joinButton.Enabled = false;
            settingsButton.Enabled = false;
            quitButton.Enabled = false;

            try
            {
                var gi = await CardsAgainstHumility.Add(CardsAgainstHumility.NewId());
                gi = CardsAgainstHumility.JoinGame(gi.Id).Result;
                StartActivity(typeof(GameActivity));
            }
            catch (Exception ex)
            {
                Log.WriteLine(LogPriority.Error, "Exception when Creating a game", ex.Message);
                throw;
            }
            finally
            {
                createButton.Enabled = true;
                joinButton.Enabled = true;
                settingsButton.Enabled = true;
                quitButton.Enabled = true;
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

