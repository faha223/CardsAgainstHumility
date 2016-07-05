using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Views.Animations;
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

        private Animation logo1Animation
        {
            get
            {
                return new TranslateAnimation(2000, 0, 0, 0)
                {
                    Duration = 150,
                    StartOffset = 1000
                };
            }
        }

        private Animation logo2Animation
        {
            get
            {
                return new TranslateAnimation(2000, 0, 0, 0)
                {
                    Duration = 150,
                    StartOffset = 1150
                };
            }
        }

        private Animation logo3Animation
        {
            get
            {
                return new TranslateAnimation(2000, 0, 0, 0)
                {
                    Duration = 150,
                    StartOffset = 1300
                };
            }
        }

        private Animation logo4Animation
        {
            get
            {
                return new TranslateAnimation(2000, 0, 0, 0)
                {
                    Duration = 150,
                    StartOffset = 1450
                };
            }
        }

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
            {
                tv.SetTypeface(UIAssets.AppFont, TypefaceStyle.Normal);
                try
                {
                    tv.StartAnimation(logo1Animation);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("Error occurred while animating the logo: {0}", ex.Message);
                }
            }
            tv = FindViewById<TextView>(Resource.Id.main_logo2);
            if (tv != null)
            {
                tv.SetTypeface(UIAssets.AppFont, TypefaceStyle.Normal);
                try
                {
                    tv.StartAnimation(logo2Animation);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("Error occurred while animating the logo: {0}", ex.Message);
                }
            }
            tv = FindViewById<TextView>(Resource.Id.main_logo3);
            if (tv != null)
            {
                tv.SetTypeface(UIAssets.AppFont, TypefaceStyle.Normal);
                try
                {
                    tv.StartAnimation(logo3Animation);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("Error occurred while animating the logo: {0}", ex.Message);
                }
            }
            tv = FindViewById<TextView>(Resource.Id.main_logo4);
            if (tv != null)
            {
                tv.SetTypeface(UIAssets.AppFont, TypefaceStyle.Normal);
                try
                {
                    tv.StartAnimation(logo4Animation);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("Error occurred while animating the logo: {0}", ex.Message);
                }
            }
            tv = FindViewById<TextView>(Resource.Id.main_logo5);
            if (tv != null)
            {
                tv.SetTypeface(UIAssets.AppFont, TypefaceStyle.Normal);
                try
                {
                    tv.StartAnimation(logo4Animation);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("Error occurred while animating the logo: {0}", ex.Message);
                }
            }

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

