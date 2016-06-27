using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Widget;
using CardsAgainstHumility.Android.Settings;
using CardsAgainstHumility.Android.UIHelpers;

using System;

namespace CardsAgainstHumility.Android
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
            CardsAgainstHumility.InitDefaultValues(new SettingsLoader(this), new NetServices.NetServices());

            dialogBuilder = new AlertDialog.Builder(this, 0);

            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            UIAssets.Initialize(Assets);

            var tv = FindViewById<TextView>(Resource.Id.main_logo1);
            if (tv != null)
                tv.SetTypeface(UIAssets.AppFont, TypefaceStyle.Normal);
            tv = FindViewById<TextView>(Resource.Id.main_logo2);
            if (tv != null)
                tv.SetTypeface(UIAssets.AppFont, TypefaceStyle.Normal);
            tv = FindViewById<TextView>(Resource.Id.main_logo3);
            if (tv != null)
                tv.SetTypeface(UIAssets.AppFont, TypefaceStyle.Normal);

            createButton = FindViewById<Button>(Resource.Id.main_btnCreateGame);
            joinButton = FindViewById<Button>(Resource.Id.main_btnJoinGame);
            settingsButton = FindViewById<Button>(Resource.Id.main_btnSettings);
            quitButton = FindViewById<Button>(Resource.Id.main_btnQuitGame);

            if (createButton != null)
            {
                createButton.Click += delegate
                {
                    StartActivity(typeof(CreateGameActivity));
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

